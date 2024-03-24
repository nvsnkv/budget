﻿using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public interface IOperationsRepository
{
    Task<IReadOnlyCollection<TrackedOperation>> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct);
    Task<Result<TrackedOperation>> Register(UnregisteredOperation operation, TrackedAccount account, CancellationToken ct);
    Task<Result<TrackedOperation>> Update(TrackedOperation operation, CancellationToken ct);
    Task<Result> Remove(TrackedOperation operation, CancellationToken ct);
}