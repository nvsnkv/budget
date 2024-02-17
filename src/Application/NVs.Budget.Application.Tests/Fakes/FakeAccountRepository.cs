using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Tests.Fakes;

internal sealed class FakeAccountRepository : IAccountsRepository
{
    private readonly List<TrackedAccount> _accounts = new();

    public Task<IReadOnlyCollection<TrackedAccount>> Get(Expression<Func<TrackedAccount, bool>> filter, CancellationToken ct)
    {
        var predicate = filter.Compile();
        var result = _accounts.Where(predicate).ToList().AsReadOnly();
        return Task.FromResult((IReadOnlyCollection<TrackedAccount>)result);
    }

    public Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var result = new TrackedAccount(id, newAccount.Name, newAccount.Bank, new[] { owner })
        {
            Version = Guid.NewGuid().ToString()
        };

        _accounts.Add(result);
        return Task.FromResult(Result.Ok(result));
    }

    public Task<Result> Update(TrackedAccount account, CancellationToken ct)
    {
        var target = _accounts.First(a => a.Id == account.Id);
        _accounts.Remove(target);
        _accounts.Add(account);

        return Task.FromResult(Result.Ok());
    }

    public Task<Result> Remove(TrackedAccount account, CancellationToken ct)
    {
        var target = _accounts.First(a => a.Id == account.Id);
        _accounts.Remove(target);

        return Task.FromResult(Result.Ok());
    }

    public void AppendAccounts(IEnumerable<TrackedAccount> accounts)
    {
        _accounts.AddRange(accounts);
    }
}
