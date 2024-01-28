using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.Errors;

namespace NVs.Budget.Domain.Tests;

public class LogbookShould
{

    [Fact]
    public void BeEmptyByDefault()
    {
        var logbook = new Logbook();
        logbook.IsEmpty.Should().BeTrue();
        logbook.Sum.Should().Be(Money.Zero());
        logbook.Transactions.Should().BeEmpty();
        logbook.From.Should().Be(DateTime.MinValue);
        logbook.Till.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void AcceptTransactionsWithSameCurrency()
    {
        var transactions = GenerateTestTransactions(5);
        var expectedFrom = transactions.Min(x => x.Timestamp);
        var expectedTill = transactions.Max(t => t.Timestamp);
        var expectedSum = transactions.Select(t => t.Amount).Aggregate((l, r) => l + r);

        var logbook = new Logbook();
        var results = transactions.Select(t => logbook.Register(t)).ToList();
        logbook.IsEmpty.Should().BeFalse();
        results.Select(r => r.IsSuccess).Should().AllBeEquivalentTo(true);

        logbook.Sum.Should().Be(expectedSum);
        logbook.From.Should().Be(expectedFrom);
        logbook.Till.Should().Be(expectedTill);
        logbook.Transactions.Should().BeEquivalentTo(transactions.OrderBy(t => t.Timestamp));
    }

    [Fact]
    public void RejectTransactionsWithDifferentCurrencies()
    {
        var fixture = new Fixture();
        var transactions = fixture.Create<Generator<Transaction>>().Take(2).ToList();
        var logbook = new Logbook();

        logbook.Register(transactions.First());
        var result = logbook.Register(transactions.Last());
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().AllBeOfType<UnexpectedCurrencyError>();

        logbook.Transactions.Should().HaveCount(1);
        logbook.Transactions.First().Should().Be(transactions.First());
    }

    private static List<Transaction> GenerateTestTransactions(int count)
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new NamedParameterBuilder<CurrencyIsoCode>("currency", fixture.Create<CurrencyIsoCode>(), false));
        var transactions = fixture.Create<Generator<Transaction>>().Take(count).ToList();
        return transactions;
    }
}
