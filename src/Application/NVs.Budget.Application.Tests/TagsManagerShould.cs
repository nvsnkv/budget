using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Services.Accounting.Tags;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

public class TagsManagerShould
{
    private readonly Fixture _fixture = new();

    public TagsManagerShould()
    {
        _fixture.Customizations.Add(new ReadableExpressionsBuilder());
    }

    [Fact]
    public void AssignTagsInAccordanceWithCriteria()
    {
        var universeTag = new Tag("Universe");
        var uniqueTag = new Tag("Unique");
        var exceptTag = new Tag("Except");

        var transaction = _fixture.Create<TrackedOperation>();
        var another = _fixture.Create<TrackedOperation>();

        var rules = new TaggingCriterion[]
        {
            new(
                ReadableExpressionsParser.Default.ParseUnaryConversion<TrackedOperation>("o => \"Universe\"").Value,
                ReadableExpressionsParser.Default.ParseUnaryPredicate<TrackedOperation>("o => true").Value
            ),
            new(
                ReadableExpressionsParser.Default.ParseUnaryConversion<TrackedOperation>("o => \"Unique\"").Value,
                ReadableExpressionsParser.Default.ParseUnaryPredicate<TrackedOperation>($"o => o.Id == Guid.Parse(\"{transaction.Id}\")").Value
                ),
            new(
                ReadableExpressionsParser.Default.ParseUnaryConversion<TrackedOperation>("o => \"Except\"").Value,
                ReadableExpressionsParser.Default.ParseUnaryPredicate<TrackedOperation>($"o => o.Id != Guid.Parse(\"{transaction.Id}\")").Value
            )
        };

        var manager = new TagsManager(rules);
        manager.GetTagsFor(transaction).Should().BeEquivalentTo(new[] { universeTag, uniqueTag });
        manager.GetTagsFor(another).Should().BeEquivalentTo(new[] { universeTag, exceptTag });
    }
}
