using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Domain.Tests;

public class TagBasedCriterionShould : CriterionShould
{
    private readonly Fixture _fixture = new();

    public TagBasedCriterionShould()
    {
        _fixture.Customizations.Add(new NamedParameterBuilder<IEnumerable<Tag>>("tags", Enumerable.Empty<Tag>(), false));
    }

    [Fact]
    public void MatchWithTransactionThatContainsAllRequiredTags()
    {
        var generator = _fixture.Create<Generator<Tag>>();
        var ruleTags = generator.Take(2).ToList();

        var transaction = _fixture.Create<Transaction>();
        foreach (var tag in ruleTags)
        {
            transaction.Tag(tag);
        }

        foreach (var tag in generator.Take(3))
        {
            transaction.Tag(tag);
        }

        var criterion = new TagBasedCriterion(_fixture.Create<string>(), ruleTags, TagBasedCriterionType.Including);
        criterion.Matched(transaction).Should().Be(true);
    }

    [Fact]
    public void MatchWithTransactionThatDoesNotContainProhibitedTags()
    {
        var generator = _fixture.Create<Generator<Tag>>();
        var badTags = generator.Take(2);
        var otherTags = generator.Take(5);

        var transaction = _fixture.Create<Transaction>();
        foreach (var tag in otherTags)
        {
            transaction.Tag(tag);
        }

        var criterion = new TagBasedCriterion(_fixture.Create<string>(), badTags, TagBasedCriterionType.Excluding);
        criterion.Matched(transaction).Should().BeTrue();
    }

    [Fact]
    public void NotMatchWithTransactionThatDoesNotContainAllRequiredTags()
    {
        var generator = _fixture.Create<Generator<Tag>>();
        var requiredTags = generator.Take(3).ToList();

        var transaction = _fixture.Create<Transaction>();
        foreach (var tag in requiredTags.Skip(1))
        {
            transaction.Tag(tag);
        }

        var criterion = new TagBasedCriterion(_fixture.Create<string>(), requiredTags, TagBasedCriterionType.Including);
        criterion.Matched(transaction).Should().BeFalse();
    }

    [Fact]
    public void NotMatchWithTransactionThatHasAtLeastOneProhibitedTag()
    {
        var badTags = _fixture.Create<Generator<Tag>>().Take(3).ToList();
        var transaction = _fixture.Create<Transaction>();
        transaction.Tag(badTags.First());

        var criterion = new TagBasedCriterion(_fixture.Create<string>(), badTags, TagBasedCriterionType.Excluding);
        criterion.Matched(transaction).Should().BeFalse();
    }

    protected override Criterion CreateCriterion(IEnumerable<Criterion> subcriteria) =>
        new TagBasedCriterion(_fixture.Create<string>(), _fixture.Create<Generator<Tag>>().Take(1), TagBasedCriterionType.Excluding, subcriteria);
}
