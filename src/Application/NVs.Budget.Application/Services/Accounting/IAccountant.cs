using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results;

namespace NVs.Budget.Application.Services.Accounting;

public interface IAccountant
{
    Task<ImportResult> ImportTransactions(IAsyncEnumerable<UnregisteredTransaction> transactions, ImportOptions options, CancellationToken ct);
    Task<Result> Update(IAsyncEnumerable<TrackedTransaction> transactions, CancellationToken ct);
    Task<Result> Delete(Expression<Func<TrackedTransaction, bool>> criteria, CancellationToken ct);
    Task<Result> RegisterTransfers(IAsyncEnumerable<UnregisteredTransfer> transfers, CancellationToken ct);
}
