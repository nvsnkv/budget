using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class AccountDoesNotExistsError(Domain.Entities.Accounts.Budget budget)
    : ErrorBase("Account with given id does not exists",
        new Dictionary<string, object>()
        {
            { nameof(TrackedBudget.Id), budget.Id }
        }
    );
