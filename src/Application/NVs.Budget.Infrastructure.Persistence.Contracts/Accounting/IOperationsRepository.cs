using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IOperationsRepository
{
    Task<IReadOnlyCollection<TrackedOperation>> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct);
    Task<Result<TrackedOperation>> Register(UnregisteredOperation operation, TrackedBudget budget, CancellationToken ct);
    Task<Result<TrackedOperation>> Update(TrackedOperation operation, CancellationToken ct);
    Task<Result> Remove(TrackedOperation operation, CancellationToken ct);
}

public interface IStreamingOperationRepository {
    IAsyncEnumerable<TrackedOperation> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct);
    IAsyncEnumerable<Result<TrackedOperation>> Update(IAsyncEnumerable<TrackedOperation> updateStream, CancellationToken ct);
}
