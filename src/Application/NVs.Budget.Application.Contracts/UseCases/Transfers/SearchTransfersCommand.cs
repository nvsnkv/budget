using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Contracts.UseCases.Transfers;

public record SearchTransfersCommand(TrackedBudget Budget, Expression<Func<TrackedOperation, bool>> Criteria, DetectionAccuracy? Accuracy): IRequest<IReadOnlyCollection<Transfer>>;
