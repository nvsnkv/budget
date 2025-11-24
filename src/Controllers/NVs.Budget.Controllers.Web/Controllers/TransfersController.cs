using System.Linq.Expressions;
using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Controllers.Web.Exceptions;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/budget/{budgetId:guid}/[controller]")]
[Produces("application/json", "application/yaml", "text/yaml")]
public class TransfersController(
    IMediator mediator,
    TransferMapper mapper,
    OperationMapper operationMapper,
    ReadableExpressionsParser parser) : Controller
{
    /// <summary>
    /// Searches for transfers in a specific budget
    /// </summary>
    /// <param name="budgetId">Budget ID from route</param>
    /// <param name="criteria">Optional filter criteria expression for operations</param>
    /// <param name="accuracy">Optional detection accuracy filter</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of transfers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<TransferResponse>), 200)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> SearchTransfers(
        [FromRoute] Guid budgetId,
        [FromQuery] string? criteria = null,
        [FromQuery] string? accuracy = null,
        CancellationToken ct = default)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {budgetId} not found or access denied") });
        }

        // Parse criteria expression
        Expression<Func<TrackedOperation, bool>>? conditions = null;
        if (!string.IsNullOrWhiteSpace(criteria))
        {
            var criteriaResult = parser.ParseUnaryPredicate<TrackedOperation>(criteria);
            if (criteriaResult.IsFailed)
            {
                return BadRequest(criteriaResult.Errors);
            }
            conditions = criteriaResult.Value.AsExpression();
        }
        else
        {
            // Default: all operations
            conditions = o => true;
        }

        // Parse accuracy if provided
        DetectionAccuracy? detectionAccuracy = null;
        if (!string.IsNullOrWhiteSpace(accuracy))
        {
            var accuracyResult = operationMapper.ParseDetectionAccuracy(accuracy);
            if (accuracyResult.IsFailed)
            {
                return BadRequest(accuracyResult.Errors);
            }
            detectionAccuracy = accuracyResult.Value;
        }

        var command = new SearchTransfersCommand(budget, conditions, detectionAccuracy);
        var transfers = await mediator.Send(command, ct);

        var response = transfers.Select(mapper.ToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Registers new transfers in a specific budget
    /// </summary>
    /// <param name="budgetId">Budget ID from route</param>
    /// <param name="request">Register transfers request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpPost]
    [Consumes("application/json", "application/yaml", "text/yaml")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> RegisterTransfers(
        [FromRoute] Guid budgetId,
        [FromBody] RegisterTransfersRequest request,
        CancellationToken ct = default)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {budgetId} not found or access denied") });
        }

        // Collect all operation IDs needed
        var operationIds = new HashSet<Guid>();
        foreach (var transferRequest in request.Transfers)
        {
            operationIds.Add(transferRequest.SourceId);
            operationIds.Add(transferRequest.SinkId);
        }

        // Get all operations by IDs
        var operationQuery = new OperationQuery(
            o => operationIds.Contains(o.Id),
            null,
            false
        );
        var listQuery = new ListOperationsQuery(operationQuery);
        
        var operations = await mediator.CreateStream(listQuery, ct)
            .ToDictionaryAsync(o => o.Id, ct);

        // Validate all operations exist
        var missingIds = operationIds.Except(operations.Keys).ToList();
        if (missingIds.Any())
        {
            return BadRequest(new List<Error> { new($"Operations not found: {string.Join(", ", missingIds)}") });
        }

        // Validate operations belong to the budget and build transfers
        var transfers = new List<UnregisteredTransfer>();
        foreach (var transferRequest in request.Transfers)
        {
            var source = operations[transferRequest.SourceId];
            var sink = operations[transferRequest.SinkId];

            // Validate operations belong to the same budget
            if (source.Budget.Id != budgetId || sink.Budget.Id != budgetId)
            {
                return BadRequest(new List<Error> { new($"Operations must belong to budget {budgetId}") });
            }

            var transferResult = mapper.FromRequest(transferRequest, source, sink);
            if (transferResult.IsFailed)
            {
                return BadRequest(transferResult.Errors);
            }

            transfers.Add(transferResult.Value);
        }

        // Build async enumerable
        async IAsyncEnumerable<UnregisteredTransfer> GetTransfersAsync()
        {
            foreach (var transfer in transfers)
            {
                yield return transfer;
            }
            await Task.CompletedTask;
        }

        var command = new RegisterTransfersCommand(GetTransfersAsync());
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Removes transfers from a specific budget
    /// </summary>
    /// <param name="budgetId">Budget ID from route</param>
    /// <param name="request">Remove transfers request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpDelete]
    [Consumes("application/json", "application/yaml", "text/yaml")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> RemoveTransfers(
        [FromRoute] Guid budgetId,
        [FromBody] RemoveTransfersRequest request,
        CancellationToken ct = default)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {budgetId} not found or access denied") });
        }

        var command = new RemoveTransfersCommand(request.SourceIds.ToArray(), request.All);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
}

