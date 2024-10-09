using System.Linq.Expressions;
using System.Reflection.Metadata;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Tests.Mocks;

internal class FakeReadOnlyAccountsRepository(TrackedBudget[] accounts) : IAccountsRepository
{
    public Task<IReadOnlyCollection<TrackedBudget>> Get(Expression<Func<TrackedBudget, bool>> filter, CancellationToken ct)
    {
        return Task.FromResult((IReadOnlyCollection<TrackedBudget>)accounts.Where(filter.Compile()).ToList());
    }

    public Task<Result<TrackedBudget>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct)
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }

    public Task<Result<TrackedBudget>> Update(TrackedBudget budget, CancellationToken ct)
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }

    public Task<Result> Remove(TrackedBudget budget, CancellationToken ct)
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }
}
