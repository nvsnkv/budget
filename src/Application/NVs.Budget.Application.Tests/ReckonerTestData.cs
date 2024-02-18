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
            .CreateMany<TrackedAccount>()
            .Take(ownedAccountsCount)
            .ToList();
        foreach (var account in OwnedAccounts)
        {
            account.AddOwner(owner);
        }

        OwnedTransactions = OwnedAccounts.SelectMany((a, i) =>
        {
            using (fixture.SetAccount(a))
            {
                return i % 2 == 0
                ? fixture.CreateWithdraws<TrackedTransaction>(ownedTransactionsPerAccount)
                : fixture.CreateIncomes<TrackedTransaction>(ownedTransactionsPerAccount);
            }

        }).ToList();

       var withdraws = fixture.CreateWithdraws<TrackedTransaction>(notOwnedTransactionsCount / 2);
       var incomes = fixture.CreateIncomes<TrackedTransaction>(notOwnedTransactionsCount / 2);
       NotOwnedTransactions = withdraws.Concat(incomes).ToList();
    }
}
