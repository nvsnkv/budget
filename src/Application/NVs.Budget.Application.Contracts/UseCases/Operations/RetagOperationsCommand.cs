using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record RetagOperationsCommand(Expression<Func<TrackedOperation, bool>> Criteria, TrackedBudget Budget, bool FromScratch) : IRequest<Result>;
