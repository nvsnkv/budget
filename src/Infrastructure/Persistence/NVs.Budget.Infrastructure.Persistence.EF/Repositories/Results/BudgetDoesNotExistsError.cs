using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class BudgetDoesNotExistsError(Domain.Entities.Accounts.Budget budget)
    : ErrorBase("Budget with given id does not exists",
        new Dictionary<string, object>
        {
            { nameof(TrackedBudget.Id), budget.Id }
        }
    );
