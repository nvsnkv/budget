using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.Errors;

namespace NVs.Budget.Domain.Tests;

public class LogbookShould
{

    protected virtual Logbook CreateLogbook() => new();

    [Fact]
    public void BeEmptyByDefault()
    {
        var logbook = CreateLogbook();
        logbook.IsEmpty.Should().BeTrue();
        logbook.Sum.Should().Be(Money.Zero());
        logbook.Transactions.Should().BeEmpty();
        logbook.From.Should().Be(DateTime.MinValue);
        logbook.Till.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void AcceptTransactionsWithSameCurrency()
    {
        var transactions = TestData.GenerateTestTransactions(5);
        var expectedFrom = transactions.Min(x => x.Timestamp);
        var expectedTill = transactions.Max(t => t.Timestamp);
        var expectedSum = transactions.Select(t => t.Amount).Aggregate((l, r) => l + r);

        var logbook = CreateLogbook();
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
        var logbook = CreateLogbook();

        logbook.Register(transactions.First());
        var result = logbook.Register(transactions.Last());
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().AllBeOfType<UnexpectedCurrencyError>();

        logbook.Transactions.Should().HaveCount(1);
        logbook.Transactions.First().Should().Be(transactions.First());
    }

    [Fact]
    public void CreateChildLogbookWithRelatedTransactions()
    {
        var transactions = TestData.GenerateTestTransactions(50);
        var min = transactions.Min(t => t.Timestamp);
        var max = transactions.Max(t => t.Timestamp);

        var childMin = min + (max - min) / 3;
        var childMax = min + (max - min) * 2 / 3;
        var expectedTransactions = transactions
            .Where(t => t.Timestamp >= childMin && t.Timestamp <= childMax)
            .OrderBy(t => t.Timestamp);

        var logbook = CreateLogbook();
        foreach (var transaction in transactions) logbook.Register(transaction);

        var child = logbook[childMin, childMax];
        child.Transactions.Should().BeEquivalentTo(expectedTransactions);
    }
}
