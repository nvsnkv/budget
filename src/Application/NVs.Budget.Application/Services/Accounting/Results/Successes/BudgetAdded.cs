using FluentResults;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class BudgetAdded : Success
{
    public BudgetAdded(Domain.Entities.Accounts.Budget budget) : base("Budget was successfully added!")
    {
        this.WithAccountId(budget);
    }
}
