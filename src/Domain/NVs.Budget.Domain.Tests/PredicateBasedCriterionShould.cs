using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class PredicateBasedCriterionShould : CriterionShould
{
    [Fact]
    public void MatchWithProperTransaction()
    {
        var fixture = new Fixture();
        var transaction = fixture.Create<Operation>();
        Func<Operation, bool> predicate = t => t == transaction;

        var criterion = new PredicateBasedCriterion(fixture.Create<string>(), predicate);
        criterion.Matched(transaction).Should().Be(true);
    }

    [Fact]
    public void NotMatchWithImproperTransaction()
    {
        var fixture = new Fixture();
        var transaction = fixture.Create<Operation>();
        Func<Operation, bool> predicate = t => t != transaction;

        var criterion = new PredicateBasedCriterion(fixture.Create<string>(), predicate);
        criterion.Matched(transaction).Should().Be(false);
    }

    [Fact]
    public void BeEquivalentToAnotherPredicateBasedCriterionWithTheSamePredicate()
    {
        var fixture = new Fixture();
        var transaction = fixture.Create<Operation>();
        Func<Operation, bool> predicate = t => t == transaction;

        var criterion = new PredicateBasedCriterion(fixture.Create<string>(), predicate);

        var dict = new Dictionary<Criterion, object?>();
        dict.Add(criterion, criterion);
        var another = new PredicateBasedCriterion(criterion.Description, predicate);

        dict[another].Should().Be(criterion);
    }

    protected override Criterion CreateCriterion(IEnumerable<Criterion> subcriteria) => new PredicateBasedCriterion(string.Empty, _ => true, subcriteria);
}
