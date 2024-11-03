using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class RetagOperationsCommandHandler(IReckoner reckoner, IAccountant accountant, IBudgetManager manager) : IRequestHandler<RetagOperationsCommand, Result>
{
    public IBudgetManager Manager { get; } = manager;

    public async Task<Result> Handle(RetagOperationsCommand request, CancellationToken cancellationToken)
    {
       var items = reckoner.GetOperations(new(request.Criteria, null, true), cancellationToken);

        //HACK: materializing operations to avoid "A command is already in progress" error using OrderBy
        items = items.OrderBy(o => o.Timestamp)
            .Where(o => o.IsRegistered);

        var mode = request.FromScratch ? TaggingMode.FromScratch : TaggingMode.Append;
        return await accountant.Update(items, request.Budget, new(null, mode), cancellationToken);
    }
}
