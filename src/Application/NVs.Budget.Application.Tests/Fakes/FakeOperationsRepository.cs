using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeOperationsRepository : FakeRepository<TrackedOperation>, IOperationsRepository, IStreamingOperationRepository
{


    public Task<Result<TrackedOperation>> Register(UnregisteredOperation operation, TrackedBudget budget, CancellationToken ct)
    {
        var trackedTransaction = new TrackedOperation(
            Guid.NewGuid(),
            operation.Timestamp,
            operation.Amount,
            operation.Description,
            budget,
            Enumerable.Empty<Tag>(),
            operation.Attributes)
        {
            Version = Guid.NewGuid().ToString()
        };

        Data.Add(trackedTransaction);
        return Task.FromResult(Result.Ok(trackedTransaction));
    }

    public Task<Result> Remove(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct)
    {
        var targets = Data.Where(filter.Compile()).ToList();
        foreach (var target in targets)
        {
            Data.Remove(target);
        }

        return Task.FromResult(Result.Ok());
    }

    IAsyncEnumerable<TrackedOperation> IStreamingOperationRepository.Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct) => DoGet(filter).Result.ToAsyncEnumerable();
    public async IAsyncEnumerable<Result<TrackedOperation>> Register(IAsyncEnumerable<UnregisteredOperation> operations, TrackedBudget budget, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var unregistered in operations.WithCancellation(ct))
        {
            yield return await Register(unregistered, budget, ct);
        }
    }

    public async IAsyncEnumerable<Result<TrackedOperation>> Update(IAsyncEnumerable<TrackedOperation> operations, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var u in operations.WithCancellation(ct))
        {
            yield return await Update(u, ct);
        }
    }
}
