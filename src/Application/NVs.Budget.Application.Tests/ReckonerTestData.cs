using AutoFixture;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

internal class ReckonerTestData
{
    public IReadOnlyList<TrackedTransaction> OwnedTransactions { get; }

    public IReadOnlyList<TrackedAccount> OwnedAccounts { get; }

    public IReadOnlyList<TrackedTransaction> NotOwnedTransactions { get; }

    public IEnumerable<TrackedAccount> AllAccounts => OwnedAccounts
        .Concat(OwnedTransactions.Select(t => t.Account as TrackedAccount))
        .Concat(NotOwnedTransactions.Select(t => t.Account as TrackedAccount))
        .Where(a => a is not null)
        .Distinct()!;

    public IEnumerable<TrackedTransaction> AllTransactions => OwnedTransactions.Concat(NotOwnedTransactions);

    public ReckonerTestData(Owner owner, int ownedAccountsCount = 2, int ownedTransactionsPerAccount = 3, int notOwnedTransactionsCount = 5)
    {
        var fixture = new Fixture();
        OwnedAccounts = fixture
            .Create<Generator<TrackedAccount>>()
            .Take(ownedAccountsCount)
            .ToList();
        foreach (var account in OwnedAccounts)
        {
            account.AddOwner(owner);
        }

        OwnedTransactions = OwnedAccounts.SelectMany((a, i) =>
        {
            var tFixture = new Fixture();
            tFixture.Customizations.Add(new NamedParameterBuilder<Account>("account", a, false));
            tFixture.Customizations.Add(i % 2 == 0
                ? new RandomNumericSequenceGenerator(-1000, -100)
                : new RandomNumericSequenceGenerator(100, 1001));

            return tFixture.Create<Generator<TrackedTransaction>>().Take(ownedTransactionsPerAccount);
        }).ToList();

        var generator = new RandomNumericSequenceGenerator(-99, -1);
        fixture.Customizations.Add(generator);
        var withdraws = fixture.Create<Generator<TrackedTransaction>>().Take(notOwnedTransactionsCount / 2);

       fixture.Customizations.Remove(generator);
       fixture.Customizations.Add(new RandomNumericSequenceGenerator(1,99));
       var incomes = fixture.Create<Generator<TrackedTransaction>>().Take(notOwnedTransactionsCount / 2);
       NotOwnedTransactions = withdraws.Concat(incomes).ToList();
    }
}
