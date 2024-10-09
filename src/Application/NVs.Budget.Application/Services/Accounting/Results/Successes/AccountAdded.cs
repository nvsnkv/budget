using FluentResults;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class AccountAdded : Success
{
    public AccountAdded(Domain.Entities.Accounts.Budget budget) : base("Account was successfully added!")
    {
        this.WithAccountId(budget);
    }
}