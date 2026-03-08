using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class SubcriteriaDrivenCriterionShould
{
    [Fact]
    public void MatchIfPreconditionMatchedAndAnySubcriterionMatched()
    {
        var operation = new Fixture().Create<Operation>();
        var criterion = new SubcriteriaDrivenCriterion(
            "group",
            [new PredicateBasedCriterion("any", o => o == operation)],
            _ => true
        );

        criterion.Matched(operation).Should().BeTrue();
    }

    [Fact]
    public void NotMatchIfPreconditionNotMatched()
    {
        var operation = new Fixture().Create<Operation>();
        var criterion = new SubcriteriaDrivenCriterion(
            "group",
            [new UniversalCriterion("all")],
            _ => false
        );

        criterion.Matched(operation).Should().BeFalse();
    }
}
