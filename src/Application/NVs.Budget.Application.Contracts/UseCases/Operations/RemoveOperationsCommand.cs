using System.Diagnostics;
using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record RemoveOperationsCommand(Expression<Func<TrackedOperation, bool>> Criteria) : IRequest<Result>;
