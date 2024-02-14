using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public interface ITransactionsRepository
{
    Task<IReadOnlyCollection<TrackedTransaction>> GetTransactions(Expression<Func<TrackedTransaction, bool>> filter, CancellationToken ct);
    Task<Result<TrackedTransaction>> Register(UnregisteredTransaction transaction, Account account, CancellationToken ct);
    Task<Result<TrackedTransaction>> Update(TrackedTransaction transaction, CancellationToken ct);
    Task<Result> Delete(Expression<Func<TrackedTransaction, bool>> filter, CancellationToken ct);
}
