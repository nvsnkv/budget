using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Transfers;

namespace NVs.Budget.Application.UseCases.Transfers;

internal class RegisterTransfersCommandHandler(IAccountant accountant) : IRequestHandler<RegisterTransfersCommand, Result>
{
    public Task<Result> Handle(RegisterTransfersCommand request, CancellationToken cancellationToken) => accountant.RegisterTransfers(request.Transfers, cancellationToken);
}
