using System.Linq.Expressions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Application.Contracts.Services;

public interface IReckoner
{
    IAsyncEnumerable<TrackedOperation> GetOperations(OperationQuery query, CancellationToken ct);
    Task<CriteriaBasedLogbook> GetLogbook(LogbookQuery query, CancellationToken ct);
    Task<IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>> GetDuplicates(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct);
}
