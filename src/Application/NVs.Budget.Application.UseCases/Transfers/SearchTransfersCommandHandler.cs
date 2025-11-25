using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Transfers;

namespace NVs.Budget.Application.UseCases.Transfers;

internal class SearchTransfersCommandHandler(IAccountant accountant) : IRequestHandler<SearchTransfersCommand, TransfersList>
{
    public Task<TransfersList> Handle(SearchTransfersCommand request, CancellationToken cancellationToken) =>  accountant.GetTransfers(request.From, request.Till, request.Budget, cancellationToken);
}
