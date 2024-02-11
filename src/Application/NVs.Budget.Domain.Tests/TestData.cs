using AutoFixture;
using NMoneys;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Domain.Tests;

internal static class TestData
{
    public static List<Transaction> GenerateTestTransactions(int count, params Tag[] tags)
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new NamedParameterBuilder<CurrencyIsoCode>("currency", fixture.Create<CurrencyIsoCode>(), false));
        var transactions = fixture.Create<Generator<Transaction>>().Take(count).ToList();
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
