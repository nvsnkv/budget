using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Controllers.Web.Tests;

public class BudgetPatchingExtensionsShould
{
    private readonly Fixture _fixture = new();
    private readonly Mapper _mapper = new(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())));

    public BudgetPatchingExtensionsShould()
    {
        _fixture.Customizations.Add(new ReadableExpressionsBuilder());
        _fixture.Inject(LogbookCriteria.Universal);
    }

    [Fact]
    public void MapBudgetToBudgetConfiguration()
    {
        var expected = _fixture.Create<TrackedBudget>();
        var configuration = _mapper.Map<BudgetConfiguration>(expected);

        var empty = new TrackedBudget(
            expected.Id,
            string.Empty,
            expected.Owners,
            Enumerable.Empty<TaggingCriterion>(),
            Enumerable.Empty<TransferCriterion>(),
            LogbookCriteria.Universal);

        var updated = empty.Patch(configuration, ReadableExpressionsParser.Default);

        updated.Should().BeSuccess();
        updated.Value.Name.Should().Be(configuration.Name);
        updated.Value.Version.Should().Be(configuration.Version);
        updated.Value.LogbookCriteria.Should().BeEquivalentTo(expected.LogbookCriteria, opts => opts.Excluding(x => x.Subcriteria));
        updated.Value.LogbookCriteria.Subcriteria.Should().BeNullOrEmpty();
        updated.Value.TaggingCriteria.Should().BeEquivalentTo(expected.TaggingCriteria);
        updated.Value.TransferCriteria.Should().BeEquivalentTo(expected.TransferCriteria);
    }
}
