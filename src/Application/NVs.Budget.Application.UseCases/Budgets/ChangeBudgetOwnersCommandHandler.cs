using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Budgets;

namespace NVs.Budget.Application.UseCases.Budgets;

internal class ChangeBudgetOwnersCommandHandler(IBudgetManager manager) : IRequestHandler<ChangeBudgetOwnersCommand, Result>
{
    public Task<Result> Handle(ChangeBudgetOwnersCommand request, CancellationToken cancellationToken) => 
        manager.ChangeOwners(request.Budget, request.Owners, cancellationToken);
}
