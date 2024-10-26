using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Transfers;

namespace NVs.Budget.Application.UseCases.Transfers;

internal class SearchTransfersCommandHandler(IReckoner reckoner, IAccountant accountant) : IRequestHandler<SearchTransfersCommand, IReadOnlyCollection<TrackedTransfer>>
{
    public async Task<IReadOnlyCollection<TrackedTransfer>> Handle(SearchTransfersCommand request, CancellationToken cancellationToken)
    {
        var operations = reckoner.GetOperations(new(request.Criteria), cancellationToken);
        //HACK: materializing operations to avoid "A command is already in progress" error
        operations = (await operations.ToListAsync(cancellationToken)).ToAsyncEnumerable();

        var results = await accountant.Update(operations, request.Budget, new(request.Accuracy, TaggingMode.Skip), cancellationToken);
        return results.Transfers;
    }
}
