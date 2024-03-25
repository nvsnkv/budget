using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Tests.Fakes;

internal sealed class FakeAccountsRepository : FakeRepository<TrackedAccount>, IAccountsRepository
{
    public Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var result = new TrackedAccount(id, newAccount.Name, newAccount.Bank, new[] { owner })
        {
            Version = Guid.NewGuid().ToString()
        };

        Data.Add(result);
        return Task.FromResult(Result.Ok(result));
    }
}
