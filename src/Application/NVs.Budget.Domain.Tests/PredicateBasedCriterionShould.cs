using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class PredicateBasedCriterionShould : CriterionShould
{
    [Fact]
    public void MatchWithProperTransaction()
    {
        var fixture = new Fixture();
        var transaction = fixture.Create<Transaction>();
        Predicate<Transaction> predicate = t => t == transaction;

        var criterion = new PredicateBasedCriterion(fixture.Create<string>(), predicate);
        criterion.Matched(transaction).Should().Be(true);
    }

    [Fact]
    public void NotMatchWithImproperTransaction()
    {
        var fixture = new Fixture();
        var transaction = fixture.Create<Transaction>();
        Predicate<Transaction> predicate = t => t != transaction;

        var criterion = new PredicateBasedCriterion(fixture.Create<string>(), predicate);
        criterion.Matched(transaction).Should().Be(false);
    }

    protected override Criterion CreateCriterion(IEnumerable<Criterion> subcriteria) => new PredicateBasedCriterion(string.Empty, _ => true, subcriteria);
}
