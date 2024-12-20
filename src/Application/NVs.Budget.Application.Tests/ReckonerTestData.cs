﻿using AutoFixture;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

internal class ReckonerTestData
{
    public IReadOnlyList<TrackedOperation> OwnedTransactions { get; }

    public IReadOnlyList<TrackedBudget> OwnedBudgets { get; }

    public IReadOnlyList<TrackedOperation> NotOwnedTransactions { get; }

    public IEnumerable<TrackedBudget> AllAccounts => OwnedBudgets
        .Concat(OwnedTransactions.Select(t => t.Budget as TrackedBudget))
        .Concat(NotOwnedTransactions.Select(t => t.Budget as TrackedBudget))
        .Where(a => a is not null)
        .Distinct()!;

    public IEnumerable<TrackedOperation> AllTransactions => OwnedTransactions.Concat(NotOwnedTransactions);

    public ReckonerTestData(Owner owner, int ownedAccountsCount = 2, int ownedTransactionsPerAccount = 3, int notOwnedTransactionsCount = 5)
    {
        var fixture = new Fixture() { Customizations = { new ReadableExpressionsBuilder() }};
        fixture.Inject(LogbookCriteria.Universal);
        OwnedBudgets = fixture
            .CreateMany<TrackedBudget>()
            .Take(ownedAccountsCount)
            .ToList();
        foreach (var budget in OwnedBudgets)
        {
            budget.AddOwner(owner);
        }

        OwnedTransactions = OwnedBudgets.SelectMany((a, i) =>
        {
            using (fixture.SetAccount(a))
            {
                return i % 2 == 0
                ? fixture.CreateWithdraws<TrackedOperation>(ownedTransactionsPerAccount)
                : fixture.CreateIncomes<TrackedOperation>(ownedTransactionsPerAccount);
            }

        }).ToList();

       var withdraws = fixture.CreateWithdraws<TrackedOperation>(notOwnedTransactionsCount / 2);
       var incomes = fixture.CreateIncomes<TrackedOperation>(notOwnedTransactionsCount / 2);
       NotOwnedTransactions = withdraws.Concat(incomes).ToList();
    }
}
