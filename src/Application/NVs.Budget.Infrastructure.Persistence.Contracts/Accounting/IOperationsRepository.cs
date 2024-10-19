using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IOperationsRepository
{
    Task<Result> Remove(TrackedOperation operation, CancellationToken ct);
}

public interface IStreamingOperationRepository {
    IAsyncEnumerable<TrackedOperation> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct);
    IAsyncEnumerable<Result<TrackedOperation>> Register(IAsyncEnumerable<UnregisteredOperation> operations,  TrackedBudget budget, CancellationToken ct);
    IAsyncEnumerable<Result<TrackedOperation>> Update(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct);
}
