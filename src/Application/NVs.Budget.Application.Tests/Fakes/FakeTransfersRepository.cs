﻿using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeTransfersRepository : FakeRepository<TrackedTransfer>, ITransfersRepository
{
    public Task<Result> Register(TrackedTransfer transfer, CancellationToken ct)
    {
        Data.Add(transfer);
        return Task.FromResult(Result.Ok());
    }

    public async Task<IEnumerable<Result>> Register(IReadOnlyCollection<TrackedTransfer> transfer, CancellationToken ct)
    {
        var results = new List<Result>();
        foreach (var t in transfer)
        {
            results.Add(await Register(t, ct));
        }

        return results;
    }
}
