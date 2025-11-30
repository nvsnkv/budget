using FluentAssertions;
using NVs.Budget.Controllers.Web.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NVs.Budget.Controllers.Web.Tests;

public class YamlDeserializationShould
{
    private readonly IDeserializer _deserializer;

    public YamlDeserializationShould()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    [Fact]
    public void DeserializeUpdateBudgetRequestFromYaml()
    {
        // Arrange
        var yaml = @"
name: Test Budget
version: v1.0
taggingCriteria:
  - tag: o => o.Description
    condition: o => o.Amount.Amount > 0
transferCriteria:
  - accuracy: Exact
    comment: Transfer
    criterion: (source, sink) => source.Amount.Amount == sink.Amount.Amount * -1
logbookCriteria:
  description: Main criteria
  isUniversal: false
  type: Any
  tags:
    - income
    - expense
  substitution: o => o.Description
  subcriteria:
    - description: Income subcriteria
      isUniversal: true
";

        // Act
        var result = _deserializer.Deserialize<UpdateBudgetRequest>(yaml);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Budget");
        result.Version.Should().Be("v1.0");
        
        result.TaggingCriteria.Should().NotBeNull();
        result.TaggingCriteria.Should().HaveCount(1);
        var taggingCriterionResponse = result.TaggingCriteria!.First();
        taggingCriterionResponse.Tag.Should().Be("o => o.Description");
        taggingCriterionResponse.Condition.Should().Be("o => o.Amount.Amount > 0");
        
        result.TransferCriteria.Should().NotBeNull();
        result.TransferCriteria.Should().HaveCount(1);
        var criterionResponse = result.TransferCriteria!.First();
        criterionResponse.Accuracy.Should().Be("Exact");
        criterionResponse.Comment.Should().Be("Transfer");
        
        result.LogbookCriteria.Should().NotBeNull();
        result.LogbookCriteria!.Description.Should().Be("Main criteria");
        result.LogbookCriteria.IsUniversal.Should().BeFalse();
        result.LogbookCriteria.Type.Should().Be("Any");
        result.LogbookCriteria.Tags.Should().Contain(new[] { "income", "expense" });
        result.LogbookCriteria.Subcriteria.Should().HaveCount(1);
        var criteriaResponse = result.LogbookCriteria.Subcriteria!.First();
        criteriaResponse.Description.Should().Be("Income subcriteria");
        criteriaResponse.IsUniversal.Should().BeTrue();
    }

    [Fact]
    public void DeserializeUpdateBudgetRequestWithEmptyCollections()
    {
        // Arrange
        var yaml = @"
name: Simple Budget
version: v1.0
logbookCriteria:
  description: Universal
  isUniversal: true
";

        // Act
        var result = _deserializer.Deserialize<UpdateBudgetRequest>(yaml);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Simple Budget");
        result.Version.Should().Be("v1.0");
        result.TaggingCriteria.Should().BeNullOrEmpty();
        result.TransferCriteria.Should().BeNullOrEmpty();
        result.LogbookCriteria.Should().NotBeNull();
        result.LogbookCriteria!.IsUniversal.Should().BeTrue();
    }

    [Fact]
    public void DeserializeTaggingCriterionResponse()
    {
        // Arrange
        var yaml = @"
tag: o => o.Description
condition: o => o.Amount.Amount > 0
";

        // Act
        var result = _deserializer.Deserialize<TaggingCriterionResponse>(yaml);

        // Assert
        result.Should().NotBeNull();
        result.Tag.Should().Be("o => o.Description");
        result.Condition.Should().Be("o => o.Amount.Amount > 0");
    }

    [Fact]
    public void DeserializeTransferCriterionResponse()
    {
        // Arrange
        var yaml = @"
accuracy: Exact
comment: Test transfer
criterion: (source, sink) => source.Amount.Amount == sink.Amount.Amount * -1
";

        // Act
        var result = _deserializer.Deserialize<TransferCriterionResponse>(yaml);

        // Assert
        result.Should().NotBeNull();
        result.Accuracy.Should().Be("Exact");
        result.Comment.Should().Be("Test transfer");
        result.Criterion.Should().Be("(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1");
    }

    [Fact]
    public void FailToDeserializeInvalidYaml()
    {
        // Arrange
        var invalidYaml = @"
name: Test Budget
version: v1.0
taggingCriteria:
  - this is not valid yaml structure
    missing proper formatting
";

        // Act & Assert
        var exception = Assert.Throws<YamlDotNet.Core.YamlException>(() =>
        {
            _deserializer.Deserialize<UpdateBudgetRequest>(invalidYaml);
        });
        
        exception.Should().NotBeNull();
    }

    [Fact]
    public void DeserializeLogbookCriteriaResponseWithNestedSubcriteria()
    {
        // Arrange
        var yaml = @"
description: Root criteria
type: Any
tags:
  - tag1
  - tag2
substitution: o => o.Description
subcriteria:
  - description: Level 1
    isUniversal: true
    subcriteria:
      - description: Level 2
        criteria: o => o.Amount.Amount > 0
";

        // Act
        var result = _deserializer.Deserialize<LogbookCriteriaResponse>(yaml);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Root criteria");
        result.Type.Should().Be("Any");
        result.Tags.Should().Contain(new[] { "tag1", "tag2" });
        result.Substitution.Should().Be("o => o.Description");
        
        result.Subcriteria.Should().HaveCount(1);
        var criteriaResponse = result.Subcriteria!.First();
        criteriaResponse.Description.Should().Be("Level 1");
        criteriaResponse.IsUniversal.Should().BeTrue();
        
        criteriaResponse.Subcriteria.Should().HaveCount(1);
        var subcriteriaResponse = criteriaResponse.Subcriteria!.First();
        subcriteriaResponse.Description.Should().Be("Level 2");
        subcriteriaResponse.Criteria.Should().Be("o => o.Amount.Amount > 0");
    }

}
