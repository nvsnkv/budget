using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class BudgetsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator):
    RepositoryBase<TrackedBudget, Guid, StoredBudget>(mapper, versionGenerator), IBudgetsRepository
{
    private readonly DbSet<StoredOwner> _owners = context.Owners;

    public async Task<Result<TrackedBudget>> Register(UnregisteredBudget newBudget, Owner owner, CancellationToken ct)
    {
        var storedOwner = await _owners.FirstOrDefaultAsync(o => o.Id == owner.Id, ct);
        if (storedOwner is null)
        {
            return Result.Fail(new OwnerIsNotRegisteredError());
        }

        var budget = new StoredBudget(Guid.Empty, newBudget.Name)
        {
            Owners = { storedOwner }
        };

        BumpVersion(budget);

        var entry = await context.Budgets.AddAsync(budget, ct);
        await context.SaveChangesAsync(ct);

        return Mapper.Map<TrackedBudget>(entry.Entity);
    }

    protected override IQueryable<StoredBudget> GetData(Expression<Func<StoredBudget, bool>> expression)
    {
        return context.Budgets.Include(a => a.Owners.Where(o => !o.Deleted)).Where(expression);
    }

    protected override Task<StoredBudget?> GetTarget(TrackedBudget item, CancellationToken ct)
    {
        return context.Budgets
            .Include(b => b.Owners.Where(o => !o.Deleted))
            .Include(b => b.TaggingCriteria)
            .Where(b => b.Id == item.Id)
            .FirstOrDefaultAsync(ct);
    }

    protected override async Task<Result<StoredBudget>> Update(StoredBudget target, TrackedBudget updated, CancellationToken ct)
    {
        var ids = updated.Owners.Select(o => o.Id).ToArray();
        var newOwners = await context.Owners.Where(o => ids.Contains(o.Id) && !o.Deleted).ToListAsync(ct);
        var missedOwners = updated.Owners.Where(o => newOwners.All(so => so.Id != o.Id)).ToList();
        if (missedOwners.Count != 0)
        {
            var reasons = missedOwners.Select(o => new EntityDoesNotExistError<Owner>(o));
            return Result.Fail(reasons);
        }

        target.Name = updated.Name;
        foreach (var toRemove in target.Owners.Except(newOwners))
        {
            target.Owners.Remove(toRemove);
        }

        foreach (var toAdd in newOwners.Except(target.Owners))
        {
            target.Owners.Add(toAdd);
        }

        target.TaggingCriteria.Clear();
        foreach (var criterion in updated.TaggingCriteria.Select(Mapper.Map<StoredTaggingCriterion>))
        {
            criterion.Budget = target;
            target.TaggingCriteria.Add(criterion);
        }

        target.TransferCriteria.Clear();
        foreach (var criterion in updated.TransferCriteria.Select(Mapper.Map<StoredTransferCriterion>))
        {
            criterion.Budget = target;
            target.TransferCriteria.Add(criterion);
        }

        target.LogbookCriteria = Mapper.Map<StoredLogbookCriteria>(updated.LogbookCriteria);

        await context.SaveChangesAsync(ct);
        return Result.Ok(target);
    }

    protected override Task Remove(StoredBudget target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }
}
