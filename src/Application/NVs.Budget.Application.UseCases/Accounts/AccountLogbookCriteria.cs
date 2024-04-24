using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.UseCases.Accounts;

public class AccountLogbookCriterion(params TrackedAccount[] accounts) : Criterion("Accounts", accounts.Select(CreateSubcriterion))
{
    private static Criterion CreateSubcriterion(TrackedAccount account) => new PredicateBasedCriterion($"{account.Bank}: {account.Name}", o => o.Account.Id == account.Id);

    public override bool Matched(Operation t) => true;
}
