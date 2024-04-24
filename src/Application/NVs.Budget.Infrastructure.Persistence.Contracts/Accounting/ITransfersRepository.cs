using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface ITransfersRepository
{
    Task<IReadOnlyCollection<TrackedTransfer>> Get(Expression<Func<TrackedTransfer, bool>> filter, CancellationToken ct);
    Task<Result> Register(TrackedTransfer transfer, CancellationToken ct);

    Task<Result> Remove(TrackedTransfer transfer, CancellationToken ct);
}
