using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Accounts;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class ListOwnedAccountsQueryHandler(IAccountManager manager) : IRequestHandler<ListOwnedAccountsQuery, IReadOnlyCollection<TrackedAccount>>
{
    public Task<IReadOnlyCollection<TrackedAccount>> Handle(ListOwnedAccountsQuery request, CancellationToken cancellationToken) => manager.GetOwnedAccounts(cancellationToken);
}
