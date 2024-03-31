using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IOwnersRepository
{
    Task<IReadOnlyCollection<TrackedOwner>> Get(Expression<Func<TrackedOwner, bool>> filter, CancellationToken ct);
    Task<Result<TrackedOwner>> Register(IUser user, CancellationToken ct);
    Task<Result<TrackedOwner>> Update(TrackedOwner owner, CancellationToken ct);
    Task<Result> Remove(TrackedOwner owner, CancellationToken ct);

    Task<TrackedOwner?> Get(IUser user, CancellationToken ct);
}
