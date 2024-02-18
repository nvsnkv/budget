using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Services.Accounting.Reckon;

public interface IReckoner
{
    IAsyncEnumerable<Transaction> GetTransactions(TransactionQuery query, CancellationToken ct);
    Task<CriteriaBasedLogbook> GetLogbook(LogbookQuery query, CancellationToken ct);
    Task<IReadOnlyCollection<IReadOnlyCollection<TrackedTransaction>>> GetDuplicates(Expression<Func<TrackedTransaction, bool>> criteria, CancellationToken ct);
}
