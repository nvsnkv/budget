using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Contracts.Services;

public interface IAccountManager
{
    Task<IReadOnlyCollection<TrackedAccount>> GetOwnedAccounts(CancellationToken ct);
    Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, CancellationToken ct);
    Task<Result> ChangeOwners(TrackedAccount account, IReadOnlyCollection<Owner> owners, CancellationToken ct);
    Task<Result> Update(TrackedAccount account, CancellationToken ct);
    Task<Result> Remove(TrackedAccount account, CancellationToken ct);
}
