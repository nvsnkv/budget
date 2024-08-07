﻿using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Results;

namespace NVs.Budget.Application.Contracts.Services;

public interface IAccountant
{
    Task<ImportResult> ImportOperations(IAsyncEnumerable<UnregisteredOperation> transactions, ImportOptions options, CancellationToken ct);
    Task<Result> Update(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct);
    Task<Result> Retag(IAsyncEnumerable<TrackedOperation> operations, bool fromScratch, CancellationToken ct);
    Task<Result> Remove(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct);
    Task<Result> RegisterTransfers(IAsyncEnumerable<UnregisteredTransfer> transfers, CancellationToken ct);
}
