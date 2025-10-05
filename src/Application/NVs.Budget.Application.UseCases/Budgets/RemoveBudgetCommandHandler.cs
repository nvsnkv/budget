using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Budgets;

namespace NVs.Budget.Application.UseCases.Budgets;

internal class RemoveBudgetCommandHandler(IBudgetManager manager) : IRequestHandler<RemoveBudgetCommand, Result>
{
    public Task<Result> Handle(RemoveBudgetCommand request, CancellationToken cancellationToken) => 
        manager.Remove(request.Budget, cancellationToken);
}
