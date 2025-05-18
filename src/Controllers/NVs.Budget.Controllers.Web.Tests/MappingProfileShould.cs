using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Controllers.Web.Tests;

public class MappingProfileShould
{
    private readonly Fixture _fixture = new();
    private readonly Mapper _mapper = new(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile(ReadableExpressionsParser.Default))));

    public MappingProfileShould()
    {
        _fixture.Inject(LogbookCriteria.Universal);
        _fixture.Customizations.Add(new ReadableExpressionsBuilder());
    }

    [Fact]
    public void MapBudgetToBudgetConfiguration()
    {
        var expected = _fixture.Create<TrackedBudget>();
        var groupedTags = expected.TaggingCriteria.GroupBy(x => x.Tag.ToString());
        var groupedTransfers = expected.TransferCriteria.GroupBy(x => x.Comment.ToString())
            .ToDictionary(x => x.Key, x => x.GroupBy(y => y.Accuracy)
            );

        var configuration = _mapper.Map<BudgetConfiguration>(expected);

        configuration.Should().NotBeNull();
        configuration.Id.Should().Be(expected.Id);
        configuration.Name.Should().Be(expected.Name);
        configuration.Version.Should().Be(expected.Version);
        configuration.Tags.Should().NotBeNull();
        configuration.Tags.Should().HaveCount(groupedTags.Count());
        foreach (var group in groupedTags)
        {
            configuration.Tags![group.Key].Should().BeEquivalentTo(group.Select(x => x.Condition.ToString()));
        }

        configuration.Transfers.Should().HaveCount(groupedTransfers.Count());
        foreach (var group in groupedTransfers)
        {
            configuration.Transfers![group.Key].Should().HaveCount(group.Value.Count());
            foreach (var transfer in group.Value)
            {
                configuration.Transfers![group.Key].Single(g => g.Accuracy == transfer.Key).Criteria.Should()
                    .BeEquivalentTo(transfer.Select(x => x.Criterion.ToString()));
            }
        }

        configuration.Logbook.Should().NotBeNull();
        configuration.Logbook.Should().HaveCount(expected.LogbookCriteria.Subcriteria?.Count ?? 0);
    }
}
