using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public interface ITransactionsRepository
{
    Task<IReadOnlyCollection<TrackedTransaction>> Get(Expression<Func<TrackedTransaction, bool>> filter, CancellationToken ct);
    Task<Result<TrackedTransaction>> Register(UnregisteredTransaction transaction, TrackedAccount account, CancellationToken ct);
    Task<Result<TrackedTransaction>> Update(TrackedTransaction transaction, CancellationToken ct);
    Task<Result> Remove(Expression<Func<TrackedTransaction, bool>> filter, CancellationToken ct);
}
