using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Accounts;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/{version:apiVersion}/[controller]")]
public class BudgetController(IBudgetManager manager ) : Controller
{
    [HttpGet]
    public Task<IReadOnlyCollection<TrackedBudget>> GetBudgets(CancellationToken ct)
    {
        return manager.GetOwnedBudgets(ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<TrackedBudget?> GetBudget(Guid id, CancellationToken ct)
    {
        var budgets = await manager.GetOwnedBudgets(ct);
        return budgets.FirstOrDefault(b => b.Id == id);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] string name, CancellationToken ct)
    {
        var result = await manager.Register(new(name), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
