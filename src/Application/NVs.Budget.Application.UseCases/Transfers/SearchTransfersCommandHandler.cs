using MediatR;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.UseCases.Transfers;

internal class SearchTransfersCommandHandler(IReckoner reckoner, IAccountant accountant) : IRequestHandler<SearchTransfersCommand, IReadOnlyCollection<Transfer>>
{
    public async Task<IReadOnlyCollection<Transfer>> Handle(SearchTransfersCommand request, CancellationToken cancellationToken)
    {
        var operations = reckoner.GetOperations(new(request.Criteria), cancellationToken);
        var results = await accountant.Update(operations, request.Budget, new(request.Accuracy, TaggingMode.Skip), cancellationToken);
        return results.Transfers;
    }
}
