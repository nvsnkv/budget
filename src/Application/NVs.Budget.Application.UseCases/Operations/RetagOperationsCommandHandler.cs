using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class RetagOperationsCommandHandler(IReckoner reckoner, IAccountant accountant) : IRequestHandler<RetagOperationsCommand, Result>
{
    public Task<Result> Handle(RetagOperationsCommand request, CancellationToken cancellationToken)
    {
        var items = reckoner.GetOperations(new(request.Criteria), cancellationToken);
        return accountant.Retag(items, request.FromScratch, cancellationToken);
    }
}
