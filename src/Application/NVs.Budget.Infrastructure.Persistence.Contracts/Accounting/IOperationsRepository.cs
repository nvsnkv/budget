using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IStreamingOperationRepository {
    IAsyncEnumerable<TrackedOperation> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct);
    IAsyncEnumerable<Result<TrackedOperation>> Register(IAsyncEnumerable<UnregisteredOperation> operations,  TrackedBudget budget, CancellationToken ct);
    IAsyncEnumerable<Result<TrackedOperation>> Update(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct);
    IAsyncEnumerable<Result<TrackedOperation>> Remove(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct);
}
