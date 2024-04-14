using System.Linq.Expressions;
using System.Reflection.Metadata;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Tests.Mocks;

internal class FakeReadOnlyAccountsRepository(TrackedAccount[] accounts) : IAccountsRepository
{
    public Task<IReadOnlyCollection<TrackedAccount>> Get(Expression<Func<TrackedAccount, bool>> filter, CancellationToken ct)
    {
        return Task.FromResult((IReadOnlyCollection<TrackedAccount>)accounts.Where(filter.Compile()).ToList());
    }

    public Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct)
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }

    public Task<Result<TrackedAccount>> Update(TrackedAccount account, CancellationToken ct)
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }

    public Task<Result> Remove(TrackedAccount account, CancellationToken ct)
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }
}
