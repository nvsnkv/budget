using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Domain.Aggregates.Plans;

namespace NVs.Budget.Application.Contracts.Services;

public interface IBudgetPlanManager
{
    Task<IReadOnlyCollection<TrackedBudgetPlan>> GetPlans(Guid budgetId, CancellationToken ct);
    Task<Result<TrackedBudgetPlan>> GetPlan(Guid budgetId, Guid planId, CancellationToken ct);
    Task<Result<TrackedBudgetPlan>> Register(Guid budgetId, UnregisteredBudgetPlan newPlan, CancellationToken ct);
    Task<Result<TrackedBudgetPlan>> Update(Guid budgetId, TrackedBudgetPlan plan, CancellationToken ct);
    Task<Result> Remove(Guid budgetId, Guid planId, CancellationToken ct);
    Task<Result<TrackedBudgetPlan>> Copy(Guid budgetId, Guid planId, string? name, CancellationToken ct);
    Task<Result<VarianceReport>> BuildVarianceReport(Guid budgetId, Guid planId, string? timeZoneId, CancellationToken ct);
}
