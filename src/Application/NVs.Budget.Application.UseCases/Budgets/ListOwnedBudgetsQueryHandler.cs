using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Accounts;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class ListOwnedBudgetsQueryHandler(IBudgetManager manager) : IRequestHandler<ListOwnedBudgetsQuery, IReadOnlyCollection<TrackedBudget>>
{
    public Task<IReadOnlyCollection<TrackedBudget>> Handle(ListOwnedBudgetsQuery request, CancellationToken cancellationToken) => manager.GetOwnedBudgets(cancellationToken);
}
