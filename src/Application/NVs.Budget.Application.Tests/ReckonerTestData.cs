using AutoFixture;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

internal class ReckonerTestData
{
    public IReadOnlyList<TrackedOperation> OwnedTransactions { get; }

    public IReadOnlyList<TrackedBudget> OwnedBudgets { get; }

    public IReadOnlyList<TrackedOperation> NotOwnedTransactions { get; }

    public IEnumerable<TrackedBudget> AllBudgets => OwnedBudgets
        .Concat(OwnedTransactions.Select(t => t.Budget as TrackedBudget))
        .Concat(NotOwnedTransactions.Select(t => t.Budget as TrackedBudget))
        .Where(a => a is not null)
        .Distinct()!;

    public IEnumerable<TrackedOperation> AllTransactions => OwnedTransactions.Concat(NotOwnedTransactions);

    public ReckonerTestData(Owner owner, int ownedBudgetsCount = 2, int ownedTransactionsPerBudget = 3, int notOwnedTransactionsCount = 5)
    {
        var fixture = new Fixture() { Customizations = { new ReadableExpressionsBuilder() }};
        fixture.Inject(LogbookCriteria.Universal);
        fixture.Inject<IEnumerable<LogbookCriteria>>([LogbookCriteria.Universal]);
        OwnedBudgets = fixture
            .CreateMany<TrackedBudget>()
            .Take(ownedBudgetsCount)
            .ToList();
        foreach (var budget in OwnedBudgets)
        {
            budget.AddOwner(owner);
        }

        OwnedTransactions = OwnedBudgets.SelectMany((a, i) =>
        {
            using (fixture.SetBudget(a))
            {
                return i % 2 == 0
                ? fixture.CreateWithdraws<TrackedOperation>(ownedTransactionsPerBudget)
                : fixture.CreateIncomes<TrackedOperation>(ownedTransactionsPerBudget);
            }

        }).ToList();

       var withdraws = fixture.CreateWithdraws<TrackedOperation>(notOwnedTransactionsCount / 2);
       var incomes = fixture.CreateIncomes<TrackedOperation>(notOwnedTransactionsCount / 2);
       NotOwnedTransactions = withdraws.Concat(incomes).ToList();
    }
}
