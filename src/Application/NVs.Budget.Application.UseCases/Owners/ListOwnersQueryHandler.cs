using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.UseCases.Owners;

internal class ListOwnersQueryHandler(IOwnersRepository repo) : IRequestHandler<ListOwnersQuery, IReadOnlyCollection<TrackedOwner>>
{
    public Task<IReadOnlyCollection<TrackedOwner>> Handle(ListOwnersQuery request, CancellationToken cancellationToken) => repo.Get(request.Criteria, cancellationToken);
}
