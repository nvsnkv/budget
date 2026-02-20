using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal sealed class FakeBudgetsRepository : FakeRepository<TrackedBudget>, IBudgetsRepository
{
    public Task<Result<TrackedBudget>> Register(UnregisteredBudget newBudget, Owner owner, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var result = new TrackedBudget(id, newBudget.Name, new[] { owner }, [], [], [LogbookCriteria.Universal])
        {
            Version = Guid.NewGuid().ToString()
        };

        Data.Add(result);
        return Task.FromResult(Result.Ok(result));
    }
}
