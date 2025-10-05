using AutoFixture;
using NMoneys;
using NVs.Budget.Domain.Entities.Operations;

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

    public static IReadOnlyList<T> CreateWithdraws<T>(this Fixture fixture, int count) where T : Operation => CreateTransactions<T>(fixture, count, -100, -1);
    public static IReadOnlyList<T> CreateIncomes<T>(this Fixture fixture, int count) where T : Operation => CreateTransactions<T>(fixture, count, 1, 100);

    public static IReadOnlyList<T> CreateTransactions<T>(this Fixture fixture, int count, long min, long max) where T : Operation
    {
        var generator = new RandomNumericSequenceGenerator(min, max);
        fixture.Customizations.Add(generator);

        var result = fixture.Create<Generator<T>>().Take(count).ToList();
        fixture.Customizations.Remove(generator);
        return result;
    }

    public static IDisposable SetCurrency(this Fixture fixture, CurrencyIsoCode code) => fixture.SetNamedParameter("currency", code);

    public static Fixture ResetCurrency(this Fixture fixture) => fixture.ResetNamedParameter<CurrencyIsoCode>("currency");

    public static IDisposable SetAccount(this Fixture fixture, Domain.Entities.Budgets.Budget budget) => fixture.SetNamedParameter(nameof(Operation.Budget).ToLower(), budget);

    public static Fixture ResetAccount(this Fixture fixture) => fixture.ResetNamedParameter<Domain.Entities.Budgets.Budget>(nameof(Operation.Budget).ToLower());

    private class Scope<T>(Fixture fixture, string name) : IDisposable
    {
        public void Dispose()
        {
            fixture.ResetNamedParameter<T>(name);
        }
    }
}
