using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal sealed class FakeBudgetPlansRepository : FakeRepository<TrackedBudgetPlan>, IBudgetPlansRepository
{
    public Task<Result<TrackedBudgetPlan>> Register(UnregisteredBudgetPlan newPlan, TrackedBudget budget, CancellationToken ct)
    {
        var result = new TrackedBudgetPlan(
            Guid.NewGuid(),
            budget.Id,
            newPlan.Name,
            newPlan.From,
            newPlan.Till,
            newPlan.CronExpression,
            newPlan.Criterion,
            newPlan.Currency,
            newPlan.Expectations,
            newPlan.ExpectedAmount,
            newPlan.Note,
            newPlan.LogbookCriteria)
        {
            Version = Guid.NewGuid().ToString()
        };

        Data.Add(result);
        return Task.FromResult(Result.Ok(result));
    }
}
