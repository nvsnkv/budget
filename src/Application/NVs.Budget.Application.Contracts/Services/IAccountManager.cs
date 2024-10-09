using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Contracts.Services;

public interface IAccountManager
{
    Task<IReadOnlyCollection<TrackedBudget>> GetOwnedAccounts(CancellationToken ct);
    Task<Result<TrackedBudget>> Register(UnregisteredAccount newAccount, CancellationToken ct);
    Task<Result> ChangeOwners(TrackedBudget budget, IReadOnlyCollection<Owner> owners, CancellationToken ct);
    Task<Result> Update(TrackedBudget budget, CancellationToken ct);
    Task<Result> Remove(TrackedBudget budget, CancellationToken ct);
}
