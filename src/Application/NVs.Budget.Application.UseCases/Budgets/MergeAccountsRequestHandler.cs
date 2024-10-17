using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Accounts;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class MergeAccountsRequestHandler(IBudgetManager manager, IReckoner reckoner, IAccountant accountant) : IRequestHandler<MergeAccountsRequest, Result>
{
    public async Task<Result> Handle(MergeAccountsRequest request, CancellationToken cancellationToken)
    {
        var budgets = (await manager.GetOwnedBudgets(cancellationToken)).Where(b => request.BudgetIds.Contains(b.Id)).ToDictionary(x => x.Id);
        var missedBudgets = request.BudgetIds.Except(budgets.Keys).ToList();
        if (missedBudgets.Any())
        {
            return Result.Fail(missedBudgets.Select(i => new BudgetDoesNotExistError(i)));
        }

        var orderedBudgets = request.BudgetIds.Select(i => budgets[i]).ToList();

        var sink = orderedBudgets.Last();
        var result = new Result();

        for (var i = 0; i < budgets.Count - 1; i++)
        {
            var source = orderedBudgets[i];

            var operations = reckoner.GetOperations(new(o => o.Budget.Id == source.Id), cancellationToken)
                .Select(o => new TrackedOperation(o.Id, o.Timestamp, o.Amount, o.Description, sink, o.Tags, o.Attributes.AsReadOnly()){ Version = o.Version });

            var updateRes = await accountant.Update(operations, cancellationToken);
            result.Reasons.AddRange(updateRes.Reasons);
            if (!updateRes.IsFailed && request.PurgeEmptyBudgets)
            {
                var accResult = await manager.Remove(source, cancellationToken);
                result.Reasons.AddRange(accResult.Reasons);
            }
        }

        return result;
    }
}
