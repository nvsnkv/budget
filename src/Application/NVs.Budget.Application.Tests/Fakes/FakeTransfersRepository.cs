using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeTransfersRepository : FakeRepository<TrackedTransfer>, ITransfersRepository
{
    public Task<Result> Register(TrackedTransfer transfer, CancellationToken ct)
    {
        Data.Add(transfer);
        return Task.FromResult(Result.Ok());
    }
}
