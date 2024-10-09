using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal sealed class FakeAccountsRepository : FakeRepository<TrackedBudget>, IAccountsRepository
{
    public Task<Result<TrackedBudget>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var result = new TrackedBudget(id, newAccount.Name, new[] { owner })
        {
            Version = Guid.NewGuid().ToString()
        };

        Data.Add(result);
        return Task.FromResult(Result.Ok(result));
    }
}
