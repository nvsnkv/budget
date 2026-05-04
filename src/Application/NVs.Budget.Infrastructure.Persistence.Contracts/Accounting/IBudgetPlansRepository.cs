using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Entities.Planning;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IBudgetPlansRepository
{
    Task<IReadOnlyCollection<TrackedBudgetPlan>> Get(Expression<Func<TrackedBudgetPlan, bool>> filter, CancellationToken ct);
    Task<Result<TrackedBudgetPlan>> Register(UnregisteredBudgetPlan newPlan, TrackedBudget budget, CancellationToken ct);
    Task<Result<TrackedBudgetPlan>> Update(TrackedBudgetPlan plan, CancellationToken ct);
    Task<Result> Remove(TrackedBudgetPlan plan, CancellationToken ct);
}
