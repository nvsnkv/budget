using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Services.Accounting.Reckon;

public interface IReckoner
{
    IAsyncEnumerable<Operation> GetTransactions(OperationQuery query, CancellationToken ct);
    Task<CriteriaBasedLogbook> GetLogbook(LogbookQuery query, CancellationToken ct);
    Task<IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>> GetDuplicates(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct);
}
