using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class CannotChangeBudgetError(TrackedOperation t) :
    ErrorBase("Cannot change budget for existing transaction: operation is not supported", new()
    {
        {"TransactionId", t.Id }
    });
