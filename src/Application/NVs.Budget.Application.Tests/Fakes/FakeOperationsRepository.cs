using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeOperationsRepository : FakeRepository<TrackedOperation>, IOperationsRepository
{
    public Task<Result<TrackedOperation>> Register(UnregisteredOperation operation, TrackedAccount account, CancellationToken ct)
    {
        var trackedTransaction = new TrackedOperation(
            Guid.NewGuid(),
            operation.Timestamp,
            operation.Amount,
            operation.Description,
            account,
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
}
