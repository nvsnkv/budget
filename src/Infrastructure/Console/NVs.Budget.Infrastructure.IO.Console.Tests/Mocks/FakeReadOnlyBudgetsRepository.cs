using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;

internal class FakeReadOnlyBudgetsRepository(TrackedBudget[] budgets) : IBudgetsRepository
{
    public Task<IReadOnlyCollection<TrackedBudget>> Get(Expression<Func<TrackedBudget, bool>> filter, CancellationToken ct)
    {
        return Task.FromResult((IReadOnlyCollection<TrackedBudget>)budgets.Where(filter.Compile()).ToList());
    }

    public Task<Result<TrackedBudget>> Register(UnregisteredBudget newBudget, Owner owner, CancellationToken ct)
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
