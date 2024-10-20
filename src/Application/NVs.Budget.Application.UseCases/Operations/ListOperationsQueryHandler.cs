using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class ListOperationsQueryHandler(IReckoner reckoner) : IStreamRequestHandler<ListOperationsQuery, TrackedOperation>
{
    public IAsyncEnumerable<TrackedOperation> Handle(ListOperationsQuery request, CancellationToken ct) => reckoner.GetOperations(request.Query, ct);
}
