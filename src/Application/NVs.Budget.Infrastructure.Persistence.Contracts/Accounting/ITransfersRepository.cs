using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface ITransfersRepository
{
    IAsyncEnumerable<TrackedTransfer> Get(Expression<Func<TrackedTransfer, bool>> filter, CancellationToken ct);
    Task<IEnumerable<Result>> Register(IReadOnlyCollection<TrackedTransfer> transfer, CancellationToken ct);

    Task<Result> Remove(TrackedTransfer transfer, CancellationToken ct);
}
