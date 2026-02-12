using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json", "application/yaml", "text/yaml")]
public class BudgetController(
    IMediator mediator, 
    BudgetMapper mapper, 
    IReadingSettingsRepository readingSettingsRepository, 
    FileReadingSettingsMapper settingsMapper) : Controller
{
    /// <summary>
    /// Gets all budgets available to the current user (owned by or shared with the user)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of available budgets</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BudgetResponse>), 200)]
    public async Task<IReadOnlyCollection<BudgetResponse>> GetAvailableBudgets(CancellationToken ct)
    {
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        return budgets.Select(mapper.ToResponse).ToList();
    }

    /// <summary>
    /// Gets a specific budget by ID
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Budget details or 404 if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BudgetResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> GetBudgetById(Guid id, CancellationToken ct)
    {
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {id} not found or access denied") });
        }

        return Ok(mapper.ToResponse(budget));
    }

    /// <summary>
    /// Registers a new budget
    /// </summary>
    /// <param name="request">Budget registration request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created budget or error details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BudgetResponse), 201)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    public async Task<IActionResult> RegisterBudget([FromBody] RegisterBudgetRequest request, CancellationToken ct)
    {
        var command = new RegisterBudgetCommand(new UnregisteredBudget(request.Name));
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = mapper.ToResponse(result.Value);
            return CreatedAtAction(nameof(GetAvailableBudgets), new { id = result.Value.Id }, response);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Changes the owners of a budget
    /// </summary>
    /// <param name="request">Change owners request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpPut("owners")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> ChangeBudgetOwners(
        [FromBody] ChangeBudgetOwnersRequest request, 
        CancellationToken ct)
    {
        // First, get the budget to validate it exists and user has access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == request.Budget.Id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {request.Budget.Id} not found or access denied") });
        }

        // Fetch actual owners by their IDs
        var owners = await mediator.Send(new ListOwnersQuery(o => request.OwnerIds.Contains(o.Id)), ct);
        
        if (owners.Count != request.OwnerIds.Count)
        {
            var foundIds = owners.Select(o => o.Id).ToList();
            var missingIds = request.OwnerIds.Except(foundIds).ToList();
            return BadRequest(new List<Error> { new($"Owners not found: {string.Join(", ", missingIds)}") });
        }

        // Create budget with user-provided version
        var budgetToUpdate = new TrackedBudget(
            request.Budget.Id,
            budget.Name,
            owners,
            budget.TaggingCriteria,
            budget.TransferCriteria,
            budget.LogbookCriteria)
        {
            Version = request.Budget.Version
        };

        var command = new ChangeBudgetOwnersCommand(budgetToUpdate, owners);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Updates a budget
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="request">Budget update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [Consumes("application/json", "application/yaml", "text/yaml")]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> UpdateBudget(
        [FromRoute] Guid id, 
        [FromBody] UpdateBudgetRequest request, 
        CancellationToken ct)
    {
        // First, get the budget to validate it exists and user has access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new Error($"Budget with ID {id} not found or access denied") });
        }

        // Parse tagging criteria if provided
        List<TaggingCriterion> taggingCriteria = budget.TaggingCriteria.ToList();
        if (request.TaggingCriteria != null && request.TaggingCriteria.Any())
        {
            taggingCriteria = new List<TaggingCriterion>();
            foreach (var tc in request.TaggingCriteria)
            {
                if (tc == null) continue; // Skip null items
                
                var parseResult = mapper.FromRequest(tc);
                if (parseResult.IsFailed)
                {
                    return BadRequest(parseResult.Errors);
                }
                taggingCriteria.Add(parseResult.Value);
            }
        }

        // Parse transfer criteria if provided
        List<TransferCriterion> transferCriteria = budget.TransferCriteria.ToList();
        if (request.TransferCriteria != null && request.TransferCriteria.Any())
        {
            transferCriteria = new List<TransferCriterion>();
            foreach (var tc in request.TransferCriteria)
            {
                if (tc == null) continue; // Skip null items
                
                var parseResult = mapper.FromRequest(tc);
                if (parseResult.IsFailed)
                {
                    return BadRequest(parseResult.Errors);
                }
                transferCriteria.Add(parseResult.Value);
            }
        }

        // Parse named logbook criteria if provided
        IReadOnlyCollection<LogbookCriteria> logbookCriteria = budget.LogbookCriteria;
        if (request.LogbookCriteria is { Count: > 0 })
        {
            var parsedCriteria = new List<LogbookCriteria>();
            foreach (var criteria in request.LogbookCriteria)
            {
                if (criteria == null)
                {
                    continue;
                }

                if (criteria.CriteriaId == Guid.Empty && criteria.IsUniversal != true)
                {
                    return BadRequest(new List<Error> { new("Each named logbook criteria must have a non-empty criteriaId") });
                }

                if (string.IsNullOrWhiteSpace(criteria.Name))
                {
                    return BadRequest(new List<Error> { new("Each named logbook criteria must have a non-empty name") });
                }

                var parseResult = mapper.FromRequest(criteria);
                if (parseResult.IsFailed)
                {
                    return BadRequest(parseResult.Errors);
                }

                parsedCriteria.Add(parseResult.Value);
            }

            if (parsedCriteria.Count == 0)
            {
                return BadRequest(new List<Error> { new("At least one named logbook criteria is required") });
            }

            var duplicateNames = parsedCriteria
                .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateNames.Count > 0)
            {
                return BadRequest(new List<Error> { new($"Logbook criteria names must be unique. Duplicates: {string.Join(", ", duplicateNames)}") });
            }

            var duplicateIds = parsedCriteria
                .GroupBy(l => l.CriteriaId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateIds.Count > 0)
            {
                return BadRequest(new List<Error> { new($"Logbook criteria IDs must be unique. Duplicates: {string.Join(", ", duplicateIds)}") });
            }

            logbookCriteria = parsedCriteria;
        }

        // Create updated budget with new properties using user-provided version
        var updatedBudget = new TrackedBudget(
            id,
            request.Name,
            budget.Owners,
            taggingCriteria,
            transferCriteria,
            logbookCriteria)
        {
            Version = request.Version
        };

        var command = new UpdateBudgetCommand(updatedBudget);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Removes a budget
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="version">Budget version for optimistic concurrency</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> RemoveBudget(
        [FromRoute] Guid id, 
        [FromQuery] string version, 
        CancellationToken ct)
    {
        // First, get the budget to validate it exists and user has access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new Error($"Budget with ID {id} not found or access denied") });
        }

        // Create budget with user-provided version
        var budgetToRemove = new TrackedBudget(
            id,
            budget.Name,
            budget.Owners,
            budget.TaggingCriteria,
            budget.TransferCriteria,
            budget.LogbookCriteria)
        {
            Version = version
        };

        var command = new RemoveBudgetCommand(budgetToRemove);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Merges multiple budgets into one
    /// </summary>
    /// <param name="request">Merge budgets request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpPost("merge")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    public async Task<IActionResult> MergeBudgets([FromBody] MergeBudgetsRequest request, CancellationToken ct)
    {
        var mergeRequest = new NVs.Budget.Application.Contracts.UseCases.Budgets.MergeBudgetsRequest(request.BudgetIds.ToList(), request.PurgeEmptyBudgets);
        var result = await mediator.Send(mergeRequest, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Gets file reading settings for a specific budget
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Reading settings or error details</returns>
    [HttpGet("{id:guid}/reading-settings")]
    [ProducesResponseType(typeof(Dictionary<string, FileReadingSettingResponse>), 200)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> GetReadingSettings(Guid id, CancellationToken ct)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {id} not found or access denied") });
        }

        // Get reading settings
        var settings = await readingSettingsRepository.GetReadingSettingsFor(budget, ct);
        var response = settingsMapper.ToResponse(settings);

        return Ok(response);
    }

    /// <summary>
    /// Updates file reading settings for a specific budget
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="request">Reading settings update request (dictionary of pattern to settings)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpPut("{id:guid}/reading-settings")]
    [ProducesResponseType(204)]
    [Consumes("application/json", "application/yaml", "text/yaml")]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> UpdateReadingSettings(
        [FromRoute] Guid id, 
        [FromBody] Dictionary<string, FileReadingSettingResponse> request, 
        CancellationToken ct)
    {
        // Validate budget access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {id} not found or access denied") });
        }

        // Parse request
        var parseResult = settingsMapper.FromRequest(request);
        if (parseResult.IsFailed)
        {
            return BadRequest(parseResult.Errors);
        }

        // Update settings
        var result = await readingSettingsRepository.UpdateReadingSettingsFor(budget, parseResult.Value, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
}

// Request models for the controller
public record BudgetIdentifier(Guid Id, string Version);

public record RegisterBudgetRequest(string Name);

public record ChangeBudgetOwnersRequest(BudgetIdentifier Budget, IReadOnlyCollection<Guid> OwnerIds);

public record MergeBudgetsRequest(IReadOnlyCollection<Guid> BudgetIds, bool PurgeEmptyBudgets);
