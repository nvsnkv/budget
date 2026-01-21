using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Budgets;

namespace NVs.Budget.Application.UseCases.Budgets;

internal class UpdateBudgetCommandHandler(IBudgetManager manager) : IRequestHandler<UpdateBudgetCommand, Result>
{
    public Task<Result> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken) => 
        manager.Update(request.Budget, cancellationToken);
}
