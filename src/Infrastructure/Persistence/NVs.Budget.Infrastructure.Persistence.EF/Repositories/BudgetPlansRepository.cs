using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class BudgetPlansRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator)
    : RepositoryBase<TrackedBudgetPlan, Guid, StoredBudgetPlan>(mapper, versionGenerator), IBudgetPlansRepository
{
    public async Task<Result<TrackedBudgetPlan>> Register(UnregisteredBudgetPlan newPlan, TrackedBudget budget, CancellationToken ct)
    {
        var storedBudget = await context.Budgets.FirstOrDefaultAsync(b => b.Id == budget.Id && !b.Deleted, ct);
        if (storedBudget is null)
        {
            return Result.Fail(new EntityDoesNotExistError<TrackedBudget>(budget));
        }

        var plan = new TrackedBudgetPlan(
            Guid.Empty,
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
            newPlan.LogbookCriteria);

        var stored = Mapper.Map<StoredBudgetPlan>(plan);
        stored.Budget = storedBudget;
        BumpVersion(stored);

        var entry = await context.BudgetPlans.AddAsync(stored, ct);
        await context.SaveChangesAsync(ct);

        return Mapper.Map<TrackedBudgetPlan>(entry.Entity);
    }

    protected override IQueryable<StoredBudgetPlan> GetData(Expression<Func<StoredBudgetPlan, bool>> expression)
    {
        return context.BudgetPlans
            .Include(p => p.Budget)
            .Where(expression);
    }

    protected override Task<StoredBudgetPlan?> GetTarget(TrackedBudgetPlan item, CancellationToken ct)
    {
        return context.BudgetPlans
            .Include(p => p.Budget)
            .Where(p => p.Id == item.Id)
            .FirstOrDefaultAsync(ct);
    }

    protected override async Task<Result<StoredBudgetPlan>> Update(StoredBudgetPlan target, TrackedBudgetPlan updated, CancellationToken ct)
    {
        target.Name = updated.Name;
        target.From = updated.From.ToUniversalTime();
        target.Till = updated.Till.ToUniversalTime();
        target.CronExpression = updated.CronExpression;
        target.CurrencyCode = updated.Currency.IsoCode;
        target.ExpectedAmount = updated.ExpectedAmount.Amount;
        target.Note = updated.Note;
        target.LogbookCriteria = Mapper.Map<StoredLogbookCriteria>(updated.LogbookCriteria) ?? StoredLogbookCriteria.Universal;
        target.Expectations = updated.Expectations.Select(Mapper.Map<StoredPlanExpectation>).ToList();

        await context.SaveChangesAsync(ct);
        return Result.Ok(target);
    }

    protected override Task Remove(StoredBudgetPlan target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }
}
