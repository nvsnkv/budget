using System.Linq.Expressions;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Owners;

public record ListOwnersQuery(Expression<Func<TrackedOwner, bool>> Criteria) : IRequest<IReadOnlyCollection<TrackedOwner>>;
