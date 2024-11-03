using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeTransfersRepository : FakeRepository<TrackedTransfer>, ITransfersRepository
{
    IAsyncEnumerable<TrackedTransfer> ITransfersRepository.Get(Expression<Func<TrackedTransfer, bool>> filter, CancellationToken ct)
    {
        return base.Get(filter, ct).Result.ToAsyncEnumerable();
    }

    public async Task<IEnumerable<Result>> Register(IReadOnlyCollection<TrackedTransfer> transfer, CancellationToken ct)
    {
        var results = new List<Result>();
        foreach (var t in transfer)
        {
            Data.Add(t);
            results.Add(await Task.FromResult(Result.Ok()));
        }

        return results;
    }
}
