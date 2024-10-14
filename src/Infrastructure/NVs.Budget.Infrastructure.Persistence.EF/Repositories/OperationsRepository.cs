using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.ExpressionVisitors;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class OperationsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator, BudgetsFinder finder) :
    RepositoryBase<TrackedOperation, Guid, StoredOperation>(mapper, versionGenerator), IOperationsRepository
{
    private readonly ExpressionSplitter _splitter = new();

    public override async Task<IReadOnlyCollection<TrackedOperation>> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedOperation, StoredOperation>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var (queryable, enumerable) = _splitter.Split(expression);
        var query = context.Operations
            .Include(t => t.Budget)
            .ThenInclude(a => a.Owners.Where(o => !o.Deleted))
            .Where(queryable);

        var items = await query.AsNoTracking().ToListAsync(ct);
        items = items.Where(enumerable).ToList();
        return Mapper.Map<List<TrackedOperation>>(items).AsReadOnly();
    }

    protected override Task<StoredOperation?> GetTarget(TrackedOperation item, CancellationToken ct)
    {
        var id = item.Id;
        return context.Operations.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    protected override async Task<Result<StoredOperation>> Update(StoredOperation target, TrackedOperation updated, CancellationToken ct)
    {
        if (target.Budget.Id != updated.Budget.Id)
        {
            var changed = await finder.FindById(updated.Budget.Id, ct);
            if (changed is null)
            {
                return Result.Fail(new BudgetDoesNotExistsError(updated.Budget));
            }

            target.Budget = changed;
        }

        target.Amount = Mapper.Map<StoredMoney>(updated.Amount);
        target.Timestamp = updated.Timestamp;
        target.Description = updated.Description;

        UpdateTags(target.Tags, updated.Tags);
        target.Attributes = updated.Attributes.ToDictionary();

        await context.SaveChangesAsync(ct);

        return Result.Ok(target);
    }

    private void UpdateTags(IList<StoredTag> targetTags, IReadOnlyCollection<Tag> updatedTags)
    {
        var updated = updatedTags.Select(t => Mapper.Map<StoredTag>(t)).ToList();
        var toRemove = targetTags.Except(updated).ToList();
        var toAdd = updated.Except(targetTags).ToList();

        foreach (var tag in toRemove)
        {
            targetTags.Remove(tag);
        }

        foreach (var tag in toAdd)
        {
            targetTags.Add(tag);
        }
    }

    protected override Task Remove(StoredOperation target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }

    public async Task<Result<TrackedOperation>> Register(UnregisteredOperation operation, TrackedBudget budget, CancellationToken ct)
    {
        var storedAccount = await finder.FindById(budget.Id, ct);
        if (storedAccount is null)
        {
            return Result.Fail(new BudgetDoesNotExistsError(budget));
        }

        var storedTransaction = new StoredOperation(Guid.Empty, operation.Timestamp.ToUniversalTime(), operation.Description)
        {
            Budget = storedAccount,
            Amount = Mapper.Map<StoredMoney>(operation.Amount),
            Attributes = new Dictionary<string, object>(operation.Attributes ?? Enumerable.Empty<KeyValuePair<string, object>>())
        };

        BumpVersion(storedTransaction);
        await context.Operations.AddAsync(storedTransaction, ct);
        await context.SaveChangesAsync(ct);

        return Mapper.Map<TrackedOperation>(storedTransaction);
    }

    protected override IQueryable<StoredOperation> GetData(Expression<Func<StoredOperation, bool>> expression)
    {
        throw new NotImplementedException();
    }
}
