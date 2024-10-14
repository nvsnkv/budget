using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class RetagOperationsCommandHandler(IReckoner reckoner, IAccountant accountant, IBudgetManager manager) : IRequestHandler<RetagOperationsCommand, Result>
{
    public async Task<Result> Handle(RetagOperationsCommand request, CancellationToken cancellationToken)
    {
        var budgets = await manager.GetOwnedBudgets(cancellationToken);
        var budget = budgets.FirstOrDefault(b => b.Id == request.BudgetId);
        if (budget is null)
        {
            return Result.Fail(new BudgetDoesNotExistError(request.BudgetId));
        }

        var items = reckoner.GetOperations(new(request.Criteria), cancellationToken);
        return await accountant.Retag(items, budget, request.FromScratch, cancellationToken);
    }
}
