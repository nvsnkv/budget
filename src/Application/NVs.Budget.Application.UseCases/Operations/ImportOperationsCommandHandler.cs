using MediatR;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class ImportOperationsCommandHandler(IAccountant accountant, IBudgetManager manager) : IRequestHandler<ImportOperationsCommand, ImportResult>
{
    public async Task<ImportResult> Handle(ImportOperationsCommand request, CancellationToken cancellationToken)
    {
        var budgets = await manager.GetOwnedBudgets(cancellationToken);
        var budget = budgets.FirstOrDefault(b => b.Id == request.BudgetId);
        if (budget is null)
        {
            return new ImportResult([], [], [], [new BudgetDoesNotExistError(request.BudgetId)]);
        }

        return await accountant.ImportOperations(request.Operations, budget, request.Options, cancellationToken);
    }
}
