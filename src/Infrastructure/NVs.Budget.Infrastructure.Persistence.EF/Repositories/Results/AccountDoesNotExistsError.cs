using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class AccountDoesNotExistsError(Domain.Entities.Accounts.Budget budget)
    : ErrorBase("Account with given id does not exists",
        new Dictionary<string, object>
        {
            { nameof(TrackedBudget.Id), budget.Id }
        }
    );
