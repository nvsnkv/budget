using AutoFixture;
using AutoMapper;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NVs.Budget.Controllers.Web.Tests;

public class BudgetConfigurationShould
{
    private readonly Fixture _fixture = new();
    private readonly Mapper _mapper = new(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())));
    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    private readonly ISerializer _yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    public BudgetConfigurationShould()
    {
        _fixture.Customizations.Add(new ReadableExpressionsBuilder());
        _fixture.Inject(LogbookCriteria.Universal);
    }

    [Fact]
    public void SerializeAndDeserializeBudgetConfiguration()
    {
        var budget = _fixture.Create<TrackedBudget>();
        budget.SetTransferCriteria(new[] { new TransferCriterion(
            DetectionAccuracy.Exact, 
            "Test", 
            ReadableExpressionsParser.Default.ParseBinaryPredicate<TrackedOperation, TrackedOperation>("(l, r) => true").Value
            )});

        var configuration = _mapper.Map<BudgetConfiguration>(budget);
        var yaml = _yamlSerializer.Serialize(configuration);
        var deserialized = _yamlDeserializer.Deserialize<BudgetConfiguration>(yaml);

        deserialized.Should().BeEquivalentTo(configuration);
    }
}