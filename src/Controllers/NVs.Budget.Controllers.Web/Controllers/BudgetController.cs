using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json", "application/yaml", "text/yaml")]
public class BudgetController(IMediator mediator) : Controller
{
    /// <summary>
    /// Gets all budgets available to the current user (owned by or shared with the user)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of available budgets</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<TrackedBudget>), 200)]
    public async Task<IReadOnlyCollection<TrackedBudget>> GetAvailableBudgets(CancellationToken ct)
    {
        return await mediator.Send(new ListOwnedBudgetsQuery(), ct);
    }

    /// <summary>
    /// Registers a new budget
    /// </summary>
    /// <param name="request">Budget registration request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created budget or error details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TrackedBudget), 201)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    public async Task<IActionResult> RegisterBudget([FromBody] RegisterBudgetRequest request, CancellationToken ct)
    {
        var command = new RegisterBudgetCommand(new UnregisteredBudget(request.Name));
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetAvailableBudgets), new { id = result.Value.Id }, result.Value);
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

        // Create updated budget with new properties using user-provided version
        var updatedBudget = new TrackedBudget(
            id,
            request.Name,
            budget.Owners,
            request.TaggingCriteria ?? budget.TaggingCriteria,
            request.TransferCriteria ?? budget.TransferCriteria,
            request.LogbookCriteria ?? budget.LogbookCriteria)
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
}

// Request models for the controller
public record BudgetIdentifier(Guid Id, string Version);

public record RegisterBudgetRequest(string Name);

public record ChangeBudgetOwnersRequest(BudgetIdentifier Budget, IReadOnlyCollection<Guid> OwnerIds);

public record UpdateBudgetRequest(
    string Name,
    string Version,
    IReadOnlyCollection<TaggingCriterion>? TaggingCriteria = null,
    IReadOnlyCollection<TransferCriterion>? TransferCriteria = null,
    LogbookCriteria? LogbookCriteria = null);

public record MergeBudgetsRequest(IReadOnlyCollection<Guid> BudgetIds, bool PurgeEmptyBudgets);
