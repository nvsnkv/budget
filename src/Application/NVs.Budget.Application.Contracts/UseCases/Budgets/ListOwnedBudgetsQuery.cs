using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Budgets;

public class ListOwnedBudgetsQuery : IRequest<IReadOnlyCollection<TrackedBudget>>;
