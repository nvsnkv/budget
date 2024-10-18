using System.Linq.Expressions;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record ListDuplicatedOperationsQuery(Expression<Func<TrackedOperation, bool>> Criteria) : IRequest<IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>>;
