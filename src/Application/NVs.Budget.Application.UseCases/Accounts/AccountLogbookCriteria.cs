using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class AccountLogbookCriterion(params TrackedBudget[] accounts) : Criterion("Accounts", accounts.Select(CreateSubcriterion))
{
    private static Criterion CreateSubcriterion(TrackedBudget budget) => new PredicateBasedCriterion($"{budget.Name}", o => o.Budget.Id == budget.Id);

    public override bool Matched(Operation t) => true;
}
