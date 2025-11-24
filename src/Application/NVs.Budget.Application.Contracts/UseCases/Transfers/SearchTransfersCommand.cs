using System.Linq.Expressions;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Transfers;

public record SearchTransfersCommand(TrackedBudget Budget, Expression<Func<TrackedOperation, bool>> Criteria, DetectionAccuracy? Accuracy): IRequest<IReadOnlyCollection<TrackedTransfer>>;
