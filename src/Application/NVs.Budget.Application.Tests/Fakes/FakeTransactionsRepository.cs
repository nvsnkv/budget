using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeTransactionsRepository : FakeRepository<TrackedTransaction>, ITransactionsRepository
{
    public Task<Result<TrackedTransaction>> Register(UnregisteredTransaction transaction, TrackedAccount account, CancellationToken ct)
    {
        var trackedTransaction = new TrackedTransaction(
            Guid.NewGuid(),
            transaction.Timestamp,
            transaction.Amount,
            transaction.Description,
            account,
            Enumerable.Empty<Tag>(),
            transaction.Attributes)
        {
            Version = Guid.NewGuid().ToString()
        };

        Data.Add(trackedTransaction);
        return Task.FromResult(Result.Ok(trackedTransaction));
    }

    public Task<Result> Remove(Expression<Func<TrackedTransaction, bool>> filter, CancellationToken ct)
    {
        var targets = Data.Where(filter.Compile()).ToList();
        foreach (var target in targets)
        {
            Data.Remove(target);
        }

        return Task.FromResult(Result.Ok());
    }
}
