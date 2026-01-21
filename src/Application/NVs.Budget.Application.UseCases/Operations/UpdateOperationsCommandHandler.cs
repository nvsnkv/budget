using MediatR;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class UpdateOperationsCommandHandler(IAccountant accountant) : IRequestHandler<UpdateOperationsCommand, UpdateResult>
{
    public Task<UpdateResult> Handle(UpdateOperationsCommand request, CancellationToken cancellationToken) => accountant.Update(request.Operations, request.Budget, request.Options, cancellationToken);
}
