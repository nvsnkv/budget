using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class CriterionShould
{
    private readonly Fixture _fixture = new();

    protected virtual Criterion CreateCriterion(IEnumerable<Criterion> subcriteria) => new UniversalCriterion(_fixture.Create<string>(), subcriteria);

    [Fact]
    public void ReturnFirstMatchedSubcriterion()
    {
        var transaction = new Fixture().Create<Operation>();
        var notMatchedCriterion = new PredicateBasedCriterion(_fixture.Create<string>(), t => t != transaction);
        var firstMatchedCriterion = new PredicateBasedCriterion(_fixture.Create<string>(), t => t == transaction);
        var secondMatchedCriterion = new UniversalCriterion(_fixture.Create<string>());

        var testCriterion = CreateCriterion(new[] { notMatchedCriterion, firstMatchedCriterion, secondMatchedCriterion });
        var subcriterion = testCriterion.GetMatchedSubcriterion(transaction);
        subcriterion.Should().NotBeNull();
        subcriterion.Should().Be(firstMatchedCriterion);
    }

    [Fact]
    public void ReturnNullIfNoMatchedSubcriterionFound()
    {
        var transaction = _fixture.Create<Operation>();

        var notMatchedCriterion = new PredicateBasedCriterion(_fixture.Create<string>(), t => t != transaction);
        var anotherNotMatchedCriterion = new PredicateBasedCriterion(_fixture.Create<string>(), t => t != transaction);

        var testCriterion = CreateCriterion(new[] { notMatchedCriterion, anotherNotMatchedCriterion });
        var subcriterion = testCriterion.GetMatchedSubcriterion(transaction);
        subcriterion.Should().BeNull();
    }

    [Fact]
    public void ReturnNullIfNoSubcriteriaProvided()
    {
        var transaction = _fixture.Create<Operation>();

        var testCriterion = CreateCriterion(Enumerable.Empty<Criterion>());
        var subcriterion = testCriterion.GetMatchedSubcriterion(transaction);
        subcriterion.Should().BeNull();
    }
}
