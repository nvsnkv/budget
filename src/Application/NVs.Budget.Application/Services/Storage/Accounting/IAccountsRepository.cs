using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public interface IAccountsRepository
{
    Task<IReadOnlyCollection<TrackedAccount>> Get(Expression<Func<TrackedAccount, bool>> filter, CancellationToken ct);
    Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct);
    Task<Result> Update(TrackedAccount account, CancellationToken ct);
    Task<Result> Remove(TrackedAccount account, CancellationToken ct);
}

public record UnregisteredAccount(string Name, string Bank);
