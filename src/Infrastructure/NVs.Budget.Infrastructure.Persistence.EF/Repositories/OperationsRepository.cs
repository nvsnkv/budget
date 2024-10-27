using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NMoneys;
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

internal class OperationsRepository(IMapper mapper, BudgetContext context, VersionGenerator generator, BudgetsFinder finder) : IStreamingOperationRepository
{
    private static readonly int BatchSize = 2000;
    private readonly ExpressionSplitter _splitter = new();

    public IAsyncEnumerable<TrackedOperation> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedOperation, StoredOperation>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var (queryable, enumerable) = _splitter.Split(expression);
        var query = context.Operations
            .AsTracking()
            .Include(t => t.Budget)
            .ThenInclude(a => a.Owners.Where(o => !o.Deleted))
            .AsSplitQuery()
            .Where(queryable);

        return query.ToAsyncEnumerable().Where(enumerable).Select(mapper.Map<TrackedOperation>);
    }

    public IAsyncEnumerable<Result<TrackedOperation>> Register(IAsyncEnumerable<UnregisteredOperation> operations, TrackedBudget budget, CancellationToken ct) =>
        ProcessStream(operations, async u =>
        {
            var storedBudget = await finder.FindById(budget.Id, ct);
            if (storedBudget is null)
            {
                return Result.Fail(new BudgetDoesNotExistsError(budget));
            }

            var storedTransaction = new StoredOperation(Guid.Empty, u.Timestamp.ToUniversalTime(), u.Description)
            {
                Budget = storedBudget,
                Amount = mapper.Map<StoredMoney>(u.Amount),
                Attributes = new Dictionary<string, object>(u.Attributes ?? Enumerable.Empty<KeyValuePair<string, object>>())
            };

            BumpVersion(storedTransaction);
            await context.Operations.AddAsync(storedTransaction, ct);

            return Result.Ok(mapper.Map<TrackedOperation>(storedTransaction));
        }, ct);

    public async IAsyncEnumerable<Result<TrackedOperation>> Update(IAsyncEnumerable<TrackedOperation> operations, [EnumeratorCancellation] CancellationToken ct)
    {
        var queue = new Queue<TrackedOperation>();
        await foreach (var u in operations.WithCancellation(ct))
        {
            queue.Enqueue(u);
            if (queue.Count > BatchSize)
            {
                foreach (var result in await UpdateItems(queue, ct))
                {
                    yield return result;
                }
            }
        }

        foreach (var result in await UpdateItems(queue, ct))
        {
            yield return result;
        }
    }

    private async Task<IEnumerable<Result<TrackedOperation>>> UpdateItems(Queue<TrackedOperation> queue, CancellationToken ct)
    {
        var ids = queue.Select(q => q.Id).ToArray();
        var targets = await context.Operations
            .Include(o => o.Amount)
            .Include(o => o.Budget)
            .Include(o => o.Tags)
            .AsSplitQuery()
            .Where(o => ids.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, ct);

        var results = new List<Result<TrackedOperation>>();

        while (queue.TryDequeue(out var u))
        {
            var target = targets.GetValueOrDefault(u.Id);

            if (target is null)
            {
                results.Add(Result.Fail(new EntityDoesNotExistError<TrackedOperation>(u)));
                continue;
            }

            if (u.Version != target.Version)
            {
                results.Add(Result.Fail(new VersionDoesNotMatchError<TrackedOperation, Guid>(u)));
                continue;
            }

            var hasChanges = false;

            if (target.Budget.Id != u.Budget.Id)
            {
                var changed = await finder.FindById(u.Budget.Id, ct);
                if (changed is null)
                {
                    results.Add(Result.Fail(new BudgetDoesNotExistError(u.Budget.Id)));
                    continue;
                }

                target.Budget = changed;
                hasChanges = true;
            }

            if (mapper.Map<Money>(target.Amount) != u.Amount)
            {
                target.Amount = mapper.Map<StoredMoney>(u.Amount);
                hasChanges = true;
            }

            if (target.Timestamp != u.Timestamp)
            {
                target.Timestamp = u.Timestamp;
                hasChanges = true;
            }

            if (target.Description != u.Description)
            {
                target.Description = u.Description;
                hasChanges = true;
            }

            hasChanges = UpdateTags(target.Tags, u.Tags) || hasChanges;
            hasChanges = UpdateDictionary(target.Attributes, u.Attributes) || hasChanges;

            if (hasChanges)
            {
                target.Version = generator.Next();
            }

            results.Add(mapper.Map<TrackedOperation>(target));
        }

        await context.SaveChangesAsync(ct);
        return results;
    }


    private bool UpdateDictionary(Dictionary<string, object> targetAttributes, IDictionary<string, object> argAttributes)
    {
        var keys = targetAttributes.Keys.Concat(argAttributes.Keys).Distinct();
        var hasChanges = false;

        foreach (var key in keys)
        {
            var hasLeft = targetAttributes.TryGetValue(key, out var left);
            var hasRight = argAttributes.TryGetValue(key, out var right);

            if (hasLeft)
            {
                if (hasRight)
                {
                    if (left != right)
                    {
                        targetAttributes[key] = right!;
                        hasChanges = true;
                    }
                }
                else
                {
                    targetAttributes.Remove(key);
                    hasChanges = true;
                }
            }
            else
            {
                if (hasRight)
                {
                    targetAttributes.Add(key, right!);
                    hasChanges = true;
                }
            }
        }

        return hasChanges;

    }

    public IAsyncEnumerable<Result<TrackedOperation>> Remove(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct) =>
        ProcessStream(operations, async v =>
        {

            var target = await context.Operations
                .FirstOrDefaultAsync(t => t.Id == v.Id, ct);

            if (target is null)
            {
                return Result.Fail(new EntityDoesNotExistError<TrackedOperation>(v));
            }

            target.Deleted = true;

            return v;
        }, ct);

    private async IAsyncEnumerable<Result<TrackedOperation>> ProcessStream<T>(IAsyncEnumerable<T> source, Func<T, ValueTask<Result<TrackedOperation>>> processor, [EnumeratorCancellation] CancellationToken ct)
    {
        var results = new Queue<Result<TrackedOperation>>();
        await foreach (var item in source.WithCancellation(ct))
        {
            results.Enqueue(await processor(item));

            if (results.Count > BatchSize)
            {
                await context.SaveChangesAsync(ct);
                while (results.TryDequeue(out var r))
                {
                    yield return r;
                }
            }
        }

        await context.SaveChangesAsync(ct);
        while (results.TryDequeue(out var r))
        {
            yield return r;
        }
    }

    private bool UpdateTags(IList<StoredTag> targetTags, IReadOnlyCollection<Tag> updatedTags)
    {
        var updated = updatedTags.Select(t => mapper.Map<StoredTag>(t)).ToList();
        var toRemove = targetTags.Except(updated).ToList();
        var toAdd = updated.Except(targetTags).ToList();

        if (toRemove.Count + toAdd.Count == 0)
        {
            return false;
        }

        foreach (var tag in toRemove)
        {
            targetTags.Remove(tag);
        }

        foreach (var tag in toAdd)
        {
            targetTags.Add(tag);
        }

        return true;
    }

    private void BumpVersion(StoredOperation target) => target.Version = generator.Next();
}
