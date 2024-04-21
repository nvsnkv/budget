using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class RemoveOperationsCommandHandler(IAccountant accountant) : IRequestHandler<RemoveOperationsCommand, Result>
{
    public Task<Result> Handle(RemoveOperationsCommand request, CancellationToken cancellationToken) => accountant.Remove(request.Criteria, cancellationToken);
}
