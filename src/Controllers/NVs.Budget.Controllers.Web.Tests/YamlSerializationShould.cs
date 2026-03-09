using FluentAssertions;
using NVs.Budget.Controllers.Web.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NVs.Budget.Controllers.Web.Tests;

public class YamlSerializationShould
{
    private readonly ISerializer _serializer;

    public YamlSerializationShould()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
    }

    [Fact]
    public void SkipNullProperties()
    {
        // Arrange
        var model = new UpdateBudgetRequest
        {
            Name = "Budget",
            Version = "v1",
            LogbookCriteria =
            [
                new LogbookCriteriaResponse
                {
                    Description = "Main",
                    Type = null,
                    Criteria = "o => o.Amount.Amount > 0",
                    Precondition = null
                }
            ]
        };

        // Act
        var yaml = _serializer.Serialize(model);

        // Assert
        yaml.Should().Contain("name: Budget");
        yaml.Should().Contain("criteria: o => o.Amount.Amount > 0");
        yaml.Should().NotContain("type:");
        yaml.Should().NotContain("precondition:");
    }
}
