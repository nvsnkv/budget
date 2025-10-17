using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Web.Exceptions;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/budget/{budgetId:guid}/[controller]")]
[Produces("application/json", "application/yaml", "text/yaml")]
public class OperationsController(
    IMediator mediator,
    OperationMapper mapper,
    ReadableExpressionsParser parser,
    ICsvFileReader csvReader,
    IReadingSettingsRepository settingsRepository) : Controller
{
    /// <summary>
    /// Gets all operations for a specific budget
    /// </summary>
    /// <param name="budgetId">Budget ID to filter operations</param>
    /// <param name="criteria">Optional filter criteria expression</param>
    /// <param name="outputCurrency">Optional output currency for conversion</param>
    /// <param name="excludeTransfers">Whether to exclude transfers from results</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of operations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IAsyncEnumerable<OperationResponse>), 200)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async IAsyncEnumerable<OperationResponse> GetOperations(
        [FromRoute] Guid budgetId,
        [FromQuery] string? criteria = null,
        [FromQuery] string? outputCurrency = null,
        [FromQuery] bool excludeTransfers = false,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            throw new NotFoundException($"Budget with ID {budgetId} not found or access denied");
        }

        // Parse user-provided criteria if specified
        Expression<Func<TrackedOperation, bool>>? conditions = null;
        if (!string.IsNullOrWhiteSpace(criteria))
        {
            var criteriaResult = parser.ParseUnaryPredicate<TrackedOperation>(criteria);
            if (criteriaResult.IsFailed)
            {
                throw new BadRequestException(criteriaResult.Errors);
            }
            
            conditions = criteriaResult.Value.AsExpression();
        }

        // Parse output currency if provided
        NMoneys.Currency? currency = null;
        if (!string.IsNullOrWhiteSpace(outputCurrency))
        {
            try
            {
                currency = NMoneys.Currency.Get(outputCurrency);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Invalid currency code: {outputCurrency}. {ex.Message}");
            }
        }

        var query = new OperationQuery(conditions, currency, excludeTransfers);
        var listQuery = new ListOperationsQuery(query);

        await foreach (var operation in mediator.CreateStream(listQuery, ct))
        {
            yield return mapper.ToResponse(operation);
        }
    }

    /// <summary>
    /// Imports new operations into a budget from CSV file
    /// </summary>
    /// <param name="budgetId">Budget ID from route</param>
    /// <param name="file">CSV file to import</param>
    /// <param name="budgetVersion">Budget version for optimistic concurrency</param>
    /// <param name="transferConfidenceLevel">Optional transfer detection confidence level</param>
    /// <param name="filePattern">Optional file pattern to match reading settings (default: .*)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Import result with success/failure details</returns>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportResultResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> ImportOperations(
        [FromRoute] Guid budgetId,
        [FromForm] IFormFile file,
        [FromForm] string budgetVersion,
        [FromForm] string? transferConfidenceLevel = null,
        [FromForm] string? filePattern = null,
        CancellationToken ct = default)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return BadRequest(new List<Error> { new("No file uploaded") });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new List<Error> { new("Only CSV files are supported") });
        }

        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            throw new NotFoundException($"Budget with ID {budgetId} not found or access denied");
        }

        // Get reading settings for this budget
        var allSettings = await settingsRepository.GetReadingSettingsFor(budget, ct);
        
        // Find matching setting by file pattern
        var pattern = filePattern ?? ".*";
        var regex = new Regex(pattern);
        FileReadingSetting? readingSetting = allSettings
            .Where(kvp => kvp.Key.IsMatch(file.FileName))
            .Select(kvp => kvp.Value)
            .FirstOrDefault();

        if (readingSetting == null)
        {
            // Try to match the provided pattern
            readingSetting = allSettings
                .Where(kvp => kvp.Key.ToString() == pattern)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
        }

        if (readingSetting == null)
        {
            return BadRequest(new List<Error> { new($"No reading settings found for file pattern '{pattern}'. Please configure reading settings for this budget first.") });
        }

        // Parse transfer confidence level
        DetectionAccuracy? transferAccuracy = null;
        if (!string.IsNullOrWhiteSpace(transferConfidenceLevel))
        {
            var accuracyResult = mapper.ParseDetectionAccuracy(transferConfidenceLevel);
            if (accuracyResult.IsFailed)
            {
                return BadRequest(accuracyResult.Errors);
            }
            transferAccuracy = accuracyResult.Value;
        }

        // Update budget version for optimistic concurrency
        budget.Version = budgetVersion;

        // Read and parse CSV file
        var parseErrors = new List<IError>();
        async IAsyncEnumerable<UnregisteredOperation> ReadOperationsAsync()
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, readingSetting.Encoding);

            await foreach (var result in csvReader.ReadUntrackedOperations(reader, readingSetting, ct))
            {
                if (result.IsSuccess)
                {
                    yield return result.Value;
                }
                else
                {
                    // Collect parsing errors
                    parseErrors.AddRange(result.Errors);
                }
            }
        }

        var options = new ImportOptions(transferAccuracy);
        var command = new ImportOperationsCommand(
            ReadOperationsAsync(),
            budget,
            options
        );

        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            // Combine parsing errors with import reasons
            var allErrors = parseErrors.Concat(result.Errors).ToList();
            
            var response = new ImportResultResponse(
                result.Operations.Select(mapper.ToResponse).ToList(),
                result.Duplicates.Select(group => group.Select(mapper.ToResponse).ToList()).ToList(),
                allErrors,
                result.Successes
            );
            return Ok(response);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Updates existing operations in a budget
    /// </summary>
    /// <param name="budgetId">Budget ID from route</param>
    /// <param name="request">Update operations request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Update result with success/failure details</returns>
    [HttpPut]
    [Consumes("application/json", "application/yaml", "text/yaml")]
    [ProducesResponseType(typeof(UpdateResultResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> UpdateOperations(
        [FromRoute] Guid budgetId,
        [FromBody] UpdateOperationsRequest request,
        CancellationToken ct)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {budgetId} not found or access denied") });
        }

        // Update budget version for optimistic concurrency
        budget.Version = request.BudgetVersion;

        // Parse operations
        var operations = new List<TrackedOperation>();
        foreach (var op in request.Operations)
        {
            var parseResult = mapper.FromRequest(op, budget);
            if (parseResult.IsFailed)
            {
                return BadRequest(parseResult.Errors);
            }
            operations.Add(parseResult.Value);
        }

        // Parse transfer confidence level
        DetectionAccuracy? transferConfidenceLevel = null;
        if (!string.IsNullOrWhiteSpace(request.TransferConfidenceLevel))
        {
            var accuracyResult = mapper.ParseDetectionAccuracy(request.TransferConfidenceLevel);
            if (accuracyResult.IsFailed)
            {
                return BadRequest(accuracyResult.Errors);
            }
            transferConfidenceLevel = accuracyResult.Value;
        }

        // Parse tagging mode
        if (!Enum.TryParse<TaggingMode>(request.TaggingMode, true, out var taggingMode))
        {
            return BadRequest(new List<Error> { new($"Invalid TaggingMode value: {request.TaggingMode}") });
        }

        var options = new UpdateOptions(transferConfidenceLevel, taggingMode);
        
        async IAsyncEnumerable<TrackedOperation> GetOperationsAsync()
        {
            foreach (var op in operations)
            {
                yield return op;
            }
            await Task.CompletedTask;
        }
        
        var command = new UpdateOperationsCommand(
            GetOperationsAsync(),
            budget,
            options
        );

        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new UpdateResultResponse(
                result.Operations.Select(mapper.ToResponse).ToList(),
                result.Errors,
                result.Successes
            );
            return Ok(response);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Removes operations matching the specified criteria from a specific budget
    /// </summary>
    /// <param name="budgetId">Budget ID from route</param>
    /// <param name="request">Remove operations request with criteria expression</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpDelete]
    [Consumes("application/json", "application/yaml", "text/yaml")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> RemoveOperations(
        [FromRoute] Guid budgetId,
        [FromBody] RemoveOperationsRequest request,
        CancellationToken ct)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {budgetId} not found or access denied") });
        }

        // Parse criteria expression
        var criteriaResult = parser.ParseUnaryPredicate<TrackedOperation>(request.Criteria);
        if (criteriaResult.IsFailed)
        {
            return BadRequest(criteriaResult.Errors);
        }

        var command = new RemoveOperationsCommand(criteriaResult.Value.AsExpression());
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
}
