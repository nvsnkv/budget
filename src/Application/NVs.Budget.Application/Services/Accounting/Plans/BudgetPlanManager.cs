using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Aggregates.Plans;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Utilities.Scheduling;

namespace NVs.Budget.Application.Services.Accounting.Plans;

internal class BudgetPlanManager(
    IBudgetPlansRepository plansRepository,
    IBudgetsRepository budgetsRepository,
    IReckoner reckoner,
    IUser currentUser) : IBudgetPlanManager
{
    private readonly Owner _currentOwner = currentUser.AsOwner();

    public async Task<IReadOnlyCollection<TrackedBudgetPlan>> GetPlans(Guid budgetId, CancellationToken ct)
    {
        var budget = await GetOwnedBudget(budgetId, ct);
        if (budget is null)
        {
            return [];
        }

        return await plansRepository.Get(p => p.BudgetId == budget.Id, ct);
    }

    public async Task<Result<TrackedBudgetPlan>> GetPlan(Guid budgetId, Guid planId, CancellationToken ct)
    {
        var budget = await GetOwnedBudget(budgetId, ct);
        if (budget is null)
        {
            return Result.Fail<TrackedBudgetPlan>($"Budget with ID {budgetId} not found or access denied");
        }

        var plan = (await plansRepository.Get(p => p.BudgetId == budget.Id && p.Id == planId, ct)).FirstOrDefault();
        return plan is null
            ? Result.Fail<TrackedBudgetPlan>($"Budget plan with ID {planId} was not found")
            : Result.Ok(plan);
    }

    public async Task<Result<TrackedBudgetPlan>> Register(Guid budgetId, UnregisteredBudgetPlan newPlan, CancellationToken ct)
    {
        var budget = await GetOwnedBudget(budgetId, ct);
        if (budget is null)
        {
            return Result.Fail<TrackedBudgetPlan>($"Budget with ID {budgetId} not found or access denied");
        }

        return await plansRepository.Register(newPlan, budget, ct);
    }

    public async Task<Result<TrackedBudgetPlan>> Update(Guid budgetId, TrackedBudgetPlan plan, CancellationToken ct)
    {
        var existing = await GetPlan(budgetId, plan.Id, ct);
        if (existing.IsFailed)
        {
            return Result.Fail<TrackedBudgetPlan>(existing.Errors);
        }

        if (plan.BudgetId != budgetId)
        {
            return Result.Fail<TrackedBudgetPlan>("Budget plan does not belong to the requested budget");
        }

        return await plansRepository.Update(plan, ct);
    }

    public async Task<Result> Remove(Guid budgetId, Guid planId, CancellationToken ct)
    {
        var existing = await GetPlan(budgetId, planId, ct);
        if (existing.IsFailed)
        {
            return existing.ToResult();
        }

        return await plansRepository.Remove(existing.Value, ct);
    }

    public async Task<Result<TrackedBudgetPlan>> Copy(Guid budgetId, Guid planId, string? name, CancellationToken ct)
    {
        var existing = await GetPlan(budgetId, planId, ct);
        if (existing.IsFailed)
        {
            return Result.Fail<TrackedBudgetPlan>(existing.Errors);
        }

        var copy = new UnregisteredBudgetPlan(
            string.IsNullOrWhiteSpace(name) ? $"{existing.Value.Name} copy" : name,
            existing.Value.From,
            existing.Value.Till,
            existing.Value.CronExpression,
            existing.Value.Criterion,
            existing.Value.Currency,
            existing.Value.Expectations.Select(CloneExpectation).ToList(),
            existing.Value.ExpectedAmount,
            existing.Value.Note,
            existing.Value.LogbookCriteria);

        return await Register(budgetId, copy, ct);
    }

    public async Task<Result<VarianceReport>> BuildVarianceReport(Guid budgetId, Guid planId, string? timeZoneId, CancellationToken ct)
    {
        var planResult = await GetPlan(budgetId, planId, ct);
        if (planResult.IsFailed)
        {
            return Result.Fail<VarianceReport>(planResult.Errors);
        }

        var plan = planResult.Value;
        var from = plan.From;
        var till = plan.Till;
        Expression<Func<TrackedOperation, bool>> filter = o =>
            o.Budget.Id == budgetId && o.Timestamp >= from && o.Timestamp < till;

        var logbook = await reckoner.GetLogbook(new LogbookQuery(plan.Criterion, plan.Currency, filter, true), ct);
        var tzResult = TimeZoneScheduling.ResolveTimeZone(timeZoneId);
        if (tzResult.IsFailed)
        {
            return Result.Fail<VarianceReport>(tzResult.Errors);
        }

        var ranges = GetVarianceRangesFromPlan(plan, tzResult.Value);
        return Result.Ok(new VarianceReport(plan, logbook, ranges));
    }

    /// <summary>One column per distinct expectation window; empty expectations → single plan-span column.</summary>
    private static IReadOnlyList<PlanRange> GetVarianceRangesFromPlan(BudgetPlan plan, TimeZoneInfo tz)
    {
        var spanHint = plan.Till - plan.From;
        if (plan.Expectations.Count == 0)
        {
            return
            [
                new PlanRange(
                    CronRangePartitioner.FormatWallCombinedRange(plan.From, plan.Till, tz),
                    plan.From,
                    plan.Till)
            ];
        }

        return plan.Expectations
            .Select(e => (e.From, e.Till))
            .Distinct()
            .OrderBy(w => w.From)
            .Select(w => new PlanRange(
                CronRangePartitioner.FormatWallRangePeriodStart(w.From, spanHint, tz),
                w.From,
                w.Till))
            .ToList();
    }

    private async Task<TrackedBudget?> GetOwnedBudget(Guid budgetId, CancellationToken ct)
    {
        return (await budgetsRepository.Get(
            b => b.Id == budgetId && b.Owners.Any(o => o.Id == _currentOwner.Id),
            ct)).FirstOrDefault();
    }

    private static PlanExpectation CloneExpectation(PlanExpectation expectation)
    {
        return new PlanExpectation(
            Guid.NewGuid(),
            expectation.ExpectedAmount,
            expectation.From,
            expectation.Till,
            expectation.SubcriterionName,
            expectation.Note);
    }

}
