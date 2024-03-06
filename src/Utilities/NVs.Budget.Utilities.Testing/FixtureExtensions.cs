using AutoFixture;
using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Utilities.Testing;

public static class FixtureExtensions
{
    public static Fixture ResetNamedParameter<T>(this Fixture fixture, string name)
    {
        var targets = fixture.Customizations.Where(c => c is NamedParameterBuilder<T> builder && builder.Name == name).ToList();
        foreach (var target in targets)
        {
            fixture.Customizations.Remove(target);
        }

        return fixture;
    }

    public static IDisposable SetNamedParameter<T>(this Fixture fixture, string name, T value)
    {
        fixture.ResetNamedParameter<T>(name);
        fixture.Customizations.Add(new NamedParameterBuilder<T>(name, value, false));
        return new Scope<T>(fixture, name);
    }

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

    public static IDisposable SetAccount(this Fixture fixture, Account account) => fixture.SetNamedParameter(nameof(Transaction.Account).ToLower(), account);

    public static Fixture ResetAccount(this Fixture fixture) => fixture.ResetNamedParameter<Account>(nameof(Transaction.Account).ToLower());

    private class Scope<T>(Fixture fixture, string name) : IDisposable
    {
        public void Dispose()
        {
            fixture.ResetNamedParameter<T>(name);
        }
    }
}
