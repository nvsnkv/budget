using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.ExpressionVisitors;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class OperationsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator, BudgetsFinder finder) :
    RepositoryBase<TrackedOperation, Guid, StoredOperation>(mapper, versionGenerator), IOperationsRepository, IStreamingOperationRepository
{
    private static readonly int BatchSize = 1000;
    private readonly ExpressionSplitter _splitter = new();

    public override async Task<IReadOnlyCollection<TrackedOperation>> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct)
    {
        var items = await DoGet(filter, false, ct).ToListAsync(ct);
        return items.AsReadOnly();
    }

    IAsyncEnumerable<TrackedOperation> IStreamingOperationRepository.Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct) => DoGet(filter, true, ct);

    private IAsyncEnumerable<TrackedOperation> DoGet(Expression<Func<TrackedOperation, bool>> filter, bool trackItems, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedOperation, StoredOperation>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var (queryable, enumerable) = _splitter.Split(expression);
        var query = context.Operations
            .Include(t => t.Budget)
            .ThenInclude(a => a.Owners.Where(o => !o.Deleted))
            .Where(queryable);

        query = !trackItems ? query.AsNoTracking() : query.AsTracking();

        return query.ToAsyncEnumerable().Where(enumerable).Select(Mapper.Map<TrackedOperation>);
    }

    public async IAsyncEnumerable<Result<TrackedOperation>> Update(IAsyncEnumerable<TrackedOperation> updateStream, [EnumeratorCancellation] CancellationToken ct)
    {
        var results = new Queue<Result<TrackedOperation>>();
        await foreach (var updated in updateStream.WithCancellation(ct))
        {
            var target = await GetTarget(updated, ct);
            if (target is null)
            {
                results.Enqueue(Result.Fail(new EntityDoesNotExistError<TrackedOperation>(updated)));
            }
            else
            {
                var updateResult = await DoUpdate(target, updated, ct);
                if (updateResult.IsSuccess)
                {
                    results.Enqueue(Mapper.Map<TrackedOperation>(updateResult.Value));
                }
                else
                {
                    results.Enqueue(updateResult.ToResult());
                }
            }

            if (results.Count > BatchSize)
            {
                await context.SaveChangesAsync(ct);
                while (results.TryDequeue(out var result))
                {
                    yield return result;
                }
            }
        }

        await context.SaveChangesAsync(ct);
        while (results.TryDequeue(out var result))
        {
            yield return result;
        }
    }

    protected override Task<StoredOperation?> GetTarget(TrackedOperation item, CancellationToken ct)
    {
        var id = item.Id;
        return context.Operations.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    protected override async Task<Result<StoredOperation>> Update(StoredOperation target, TrackedOperation updated, CancellationToken ct)
    {
        var result = await DoUpdate(target, updated, ct);

        await context.SaveChangesAsync(ct);

        return result;
    }

    private async Task<Result<StoredOperation>> DoUpdate(StoredOperation target, TrackedOperation updated, CancellationToken ct)
    {
        if (target.Budget.Id != updated.Budget.Id)
        {
            var changed = await finder.FindById(updated.Budget.Id, ct);
            if (changed is null)
            {
                return Result.Fail(new BudgetDoesNotExistError(updated.Budget.Id));
            }

            target.Budget = changed;
        }

        target.Amount = Mapper.Map<StoredMoney>(updated.Amount);
        target.Timestamp = updated.Timestamp;
        target.Description = updated.Description;

        UpdateTags(target.Tags, updated.Tags);
        target.Attributes = updated.Attributes.ToDictionary();
        return target;
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
