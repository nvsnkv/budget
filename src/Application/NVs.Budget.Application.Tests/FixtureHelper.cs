using AutoFixture;
using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

internal static class FixtureHelper
{
    public static IReadOnlyList<T> CreateWithdraws<T>(this Fixture fixture, int count) where T : Transaction => CreateTransactions<T>(fixture, count, -100, -1);
    public static IReadOnlyList<T> CreateIncomes<T>(this Fixture fixture, int count) where T : Transaction => CreateTransactions<T>(fixture, count, 1, 100);

    public static IReadOnlyList<T> CreateTransactions<T>(this Fixture fixture, int count, long min, long max) where T : Transaction
    {
        var generator = new RandomNumericSequenceGenerator(min, max);
        fixture.Customizations.Add(generator);

        var result = fixture.Create<Generator<T>>().Take(count).ToList();
        fixture.Customizations.Remove(generator);
        return result;
    }

    public static IDisposable SetCurrency(this Fixture fixture, CurrencyIsoCode code) => fixture.SetNamedParameter("currency", code);

    public static Fixture ResetCurrency(this Fixture fixture) => fixture.ResetNamedParameter<CurrencyIsoCode>("currency");

    public static IDisposable SetAccount(this Fixture fixture, Account account) => fixture.SetNamedParameter("account", account);

    public static Fixture ResetAccount(this Fixture fixture) => fixture.ResetNamedParameter<Account>("account");


}
