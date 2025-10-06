using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
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
    /// <param name="id">Budget ID</param>
    /// <param name="request">Change owners request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpPut("{id:guid}/owners")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> ChangeBudgetOwners(
        [FromRoute] Guid id, 
        [FromBody] ChangeBudgetOwnersRequest request, 
        CancellationToken ct)
    {
        // First, get the budget to validate it exists and user has access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new($"Budget with ID {id} not found or access denied") });
        }

        // For now, we'll create owners with placeholder names since we only have IDs
        // In a real application, you might want to fetch owner details from a service
        var owners = request.OwnerIds.Select(id => new Owner(id, $"Owner-{id}")).ToList();
        var command = new ChangeBudgetOwnersCommand(budget, owners);
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

        // Create updated budget with new properties
        var updatedBudget = new TrackedBudget(
            budget.Id,
            request.Name,
            budget.Owners,
            request.TaggingCriteria ?? budget.TaggingCriteria,
            request.TransferCriteria ?? budget.TransferCriteria,
            request.LogbookCriteria ?? budget.LogbookCriteria)
        {
            Version = budget.Version
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
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error details</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 400)]
    [ProducesResponseType(typeof(IEnumerable<Error>), 404)]
    public async Task<IActionResult> RemoveBudget([FromRoute] Guid id, CancellationToken ct)
    {
        // First, get the budget to validate it exists and user has access
        var budgets = await mediator.Send(new ListOwnedBudgetsQuery(), ct);
        var budget = budgets.FirstOrDefault(b => b.Id == id);
        
        if (budget == null)
        {
            return NotFound(new List<Error> { new Error($"Budget with ID {id} not found or access denied") });
        }

        var command = new RemoveBudgetCommand(budget);
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
public record RegisterBudgetRequest(string Name);

public record ChangeBudgetOwnersRequest(IReadOnlyCollection<Guid> OwnerIds);

public record UpdateBudgetRequest(
    string Name,
    IReadOnlyCollection<TaggingCriterion>? TaggingCriteria = null,
    IReadOnlyCollection<TransferCriterion>? TransferCriteria = null,
    LogbookCriteria? LogbookCriteria = null);

public record MergeBudgetsRequest(IReadOnlyCollection<Guid> BudgetIds, bool PurgeEmptyBudgets);
