using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeTransfersRepository : FakeRepository<TrackedTransfer>, ITransfersRepository
{
    public Task<Result> Register(TrackedTransfer transfer, CancellationToken ct)
    {
        Data.Add(transfer);
        return Task.FromResult(Result.Ok());
    }
}
