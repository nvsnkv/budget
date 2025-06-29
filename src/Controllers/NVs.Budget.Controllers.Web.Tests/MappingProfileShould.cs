using System.Globalization;
using System.Text.RegularExpressions;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Controllers.Web.Tests;

public class MappingProfileShould
{
    private readonly Fixture _fixture = new();
    private readonly Mapper _mapper = new(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())));

    public MappingProfileShould()
    {
        _fixture.Inject(LogbookCriteria.Universal);
        _fixture.Customizations.Add(new ReadableExpressionsBuilder());
    }

    [Fact]
    public void MapBudgetToBudgetConfiguration()
    {
        var expected = _fixture.Create<TrackedBudget>();
        var groupedTags = expected.TaggingCriteria.GroupBy(x => x.Tag.ToString()).ToList();
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

    [Fact]
    public void MapTaggingCriteriaToDictionary()
    {
        var source = _fixture.CreateMany<TaggingCriterion>().ToList();
        var result = _mapper.Map<IDictionary<string, IEnumerable<string>>>(source);

        result.Should().NotBeNull();
        result.Should().HaveCount(source.GroupBy(x => x.Tag.ToString()).Count());
        foreach (var group in source.GroupBy(x => x.Tag.ToString()))
        {
            result[group.Key].Should().BeEquivalentTo(group.Select(x => x.Condition.ToString()));
        }
    }

    [Fact]
    public void MapTransferCriteriaToDictionary()
    {
        var source = _fixture.CreateMany<TransferCriterion>().ToList();
        var result = _mapper.Map<IDictionary<string, IEnumerable<TransferCriterionExpression>>>(source);

        result.Should().NotBeNull();
        result.Should().HaveCount(source.GroupBy(x => x.Comment).Count());
        foreach (var group in source.GroupBy(x => x.Comment))
        {
            var expressions = result[group.Key].ToList();
            expressions.Should().HaveCount(group.GroupBy(x => x.Accuracy).Count());
            
            foreach (var accuracyGroup in group.GroupBy(x => x.Accuracy))
            {
                var expression = expressions.Single(e => e.Accuracy == accuracyGroup.Key);
                expression.Criteria.Should().BeEquivalentTo(accuracyGroup.Select(x => x.Criterion.ToString()));
            }
        }
    }

    [Fact]
    public void MapLogbookCriteriaToDictionary()
    {
        var source = _fixture.Create<LogbookCriteria>();
        var result = _mapper.Map<IDictionary<string, LogbookCriteriaExpression>>(source);

        result.Should().NotBeNull();
        if (source.Subcriteria != null)
        {
            result.Should().HaveCount(source.Subcriteria.Count);
            foreach (var subcriteria in source.Subcriteria)
            {
                var expression = result[subcriteria.Description];
                expression.Type.Should().Be(subcriteria.Type);
                expression.Tags.Should().BeEquivalentTo(subcriteria.Tags?.Select(t => t.ToString()));
                expression.Substitution.Should().Be(subcriteria.Substitution?.ToString());
                expression.Criteria.Should().Be(subcriteria.Criteria?.ToString());
            }
        }
        else
        {
            result.Should().BeEmpty();
        }
    }

    [Fact]
    public void MapFileReadingSettingsToCsvFileReadingConfiguration()
    {
        var source = _fixture.Create<Dictionary<Regex, FileReadingSetting>>();
        var result = _mapper.Map<IDictionary<string, CsvFileReadingConfiguration>>(source);

        result.Should().NotBeNull();
        result.Should().HaveCount(source.Count);
        foreach (var (key, value) in source)
        {
            var actual = result[key.ToString()];
            if (value.Culture == CultureInfo.InvariantCulture) {
                actual.CultureCode.Should().BeNull();
            }
            else {
                actual.CultureCode.Should().Be(value.Culture.Name);
            }
            
            actual.EncodingName.Should().Be(value.Encoding.EncodingName);
            actual.Fields.Should().BeEquivalentTo(value.Fields);
            actual.Attributes.Should().BeEquivalentTo(value.Attributes);

            actual.Validation.Should().HaveCount(value.Validation.Count);
            var expected = value.Validation.ToList();
            for (var i = 0; i < actual.Validation.Length; i++)
            {
                var validation = actual.Validation[i];
                validation.Pattern.Should().Be(expected[i].Pattern);
                validation.Value.Should().Be(expected[i].Value);
                validation.ErrorMessage.Should().Be(expected[i].ErrorMessage);
                switch (expected[i].Condition)
                {
                    case ValidationRule.ValidationCondition.Equals:
                        validation.Condition.Should().Be(CsvValidationCondition.Equals);
                        break;
                    case ValidationRule.ValidationCondition.NotEquals:
                        validation.Condition.Should().Be(CsvValidationCondition.NotEquals);
                        break;
                    default:
                        throw new ArgumentException("Invalid condition");
                }
            }
        }
    }
}
