using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IBudgetsRepository
{
    Task<IReadOnlyCollection<TrackedBudget>> Get(Expression<Func<TrackedBudget, bool>> filter, CancellationToken ct);
    Task<Result<TrackedBudget>> Register(UnregisteredBudget newBudget, Owner owner, CancellationToken ct);
    Task<Result<TrackedBudget>> Update(TrackedBudget budget, CancellationToken ct);
    Task<Result> Remove(TrackedBudget budget, CancellationToken ct);
}
