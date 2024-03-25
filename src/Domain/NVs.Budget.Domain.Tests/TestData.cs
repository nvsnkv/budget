using AutoFixture;
using NMoneys;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Domain.Tests;

internal static class TestData
{
    public static List<Operation> GenerateTestTransactions(int count, params Tag[] tags)
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new NamedParameterBuilder<CurrencyIsoCode>("currency", fixture.Create<CurrencyIsoCode>(), false));
        var transactions = fixture.Create<Generator<Operation>>().Take(count).ToList();
        foreach (var transaction in transactions)
        {
            foreach (var tag in tags)
            {
                transaction.Tag(tag);
            }
        }
        return transactions;
    }
}
