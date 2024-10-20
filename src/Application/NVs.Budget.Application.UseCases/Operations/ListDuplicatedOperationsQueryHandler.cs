using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class ListDuplicatedOperationsQueryHandler(IReckoner reckoner) : IRequestHandler<ListDuplicatedOperationsQuery, IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>>
{
    public Task<IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>> Handle(ListDuplicatedOperationsQuery request, CancellationToken ct) => reckoner.GetDuplicates(request.Criteria, ct);
}
