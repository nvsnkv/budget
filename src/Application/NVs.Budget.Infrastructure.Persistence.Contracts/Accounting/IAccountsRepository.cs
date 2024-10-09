using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IAccountsRepository
{
    Task<IReadOnlyCollection<TrackedBudget>> Get(Expression<Func<TrackedBudget, bool>> filter, CancellationToken ct);
    Task<Result<TrackedBudget>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct);
    Task<Result<TrackedBudget>> Update(TrackedBudget budget, CancellationToken ct);
    Task<Result> Remove(TrackedBudget budget, CancellationToken ct);
}
