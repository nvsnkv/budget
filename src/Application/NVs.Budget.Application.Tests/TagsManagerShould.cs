using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Tags;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Tests;

public class TagsManagerShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void AssignTagsInAccordanceWithCriteria()
    {
        var universeTag = new Tag("Universe");
        var uniqueTag = new Tag("Unique");
        var exceptTag = new Tag("Except");

        var transaction = _fixture.Create<TrackedOperation>();
        var another = _fixture.Create<TrackedOperation>();

        var criteria = new TaggingCriterion[]
        {
            new(universeTag, _ => true),
            new(uniqueTag, t => t == transaction),
            new(exceptTag, t => t != transaction)
        };

        var manager = new TagsManager(criteria);
        manager.GetTags(transaction).Should().BeEquivalentTo(new[] { universeTag, uniqueTag });
        manager.GetTags(another).Should().BeEquivalentTo(new[] { universeTag, exceptTag });
    }
}
