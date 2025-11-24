using FluentResults;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class BudgetAdded : Success
{
    public BudgetAdded(Domain.Entities.Budgets.Budget budget) : base("Budget was successfully added!")
    {
        this.WithBudgetId(budget);
    }
}
