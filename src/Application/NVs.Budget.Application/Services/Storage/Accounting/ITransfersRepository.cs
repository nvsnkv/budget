﻿using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public interface ITransfersRepository
{
    Task<IReadOnlyCollection<TrackedTransfer>> GetTransfersFor(IEnumerable<TrackedTransaction> transactions, CancellationToken ct);
    Task<Result> Track(TrackedTransfer transfer, CancellationToken ct);
}
