using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Tests;

public class BudgetMapperShould
{
    private readonly BudgetMapper _mapper;
    private readonly ReadableExpressionsParser _parser;

    public BudgetMapperShould()
    {
        _parser = ReadableExpressionsParser.Default.RegisterAdditionalTypes(
            typeof(TrackedOperation),
            typeof(Operation)
        );
        _mapper = new BudgetMapper(_parser);
    }

    [Fact]
    public void MapAllBudgetPropertiesToResponse()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var owner = new Owner(Guid.NewGuid(), "Test Owner");
        
        var tagExpression = _parser.ParseUnaryConversion<TrackedOperation>("o => \"Shopping\"").Value;
        var conditionExpression = _parser.ParseUnaryPredicate<TrackedOperation>("o => o.Amount.Amount > 100").Value;
        var taggingCriterion = new TaggingCriterion(tagExpression, conditionExpression);

        var transferExpression = _parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(
            "(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1"
        ).Value;
        var transferCriterion = new TransferCriterion(DetectionAccuracy.Exact, "Transfer", transferExpression);

        var logbookCriteria = LogbookCriteria.Universal;

        var budget = new TrackedBudget(
            budgetId,
            "Test Budget",
            new[] { owner },
            new[] { taggingCriterion },
            new[] { transferCriterion },
            logbookCriteria)
        {
            Version = "v1"
        };

        // Act
        var response = _mapper.ToResponse(budget);

        // Assert
        response.Id.Should().Be(budgetId);
        response.Name.Should().Be("Test Budget");
        response.Version.Should().Be("v1");
        response.Owners.Should().HaveCount(1);
        response.Owners.First().Should().Be(owner);
        response.TaggingCriteria.Should().HaveCount(1);
        response.TransferCriteria.Should().HaveCount(1);
        response.LogbookCriteria.Should().NotBeNull();
        response.LogbookCriteria.Should().HaveCount(1);
    }

    [Fact]
    public void ConvertTaggingCriterionExpressionsToStrings()
    {
        // Arrange
        var owner = new Owner(Guid.NewGuid(), "Test Owner");
        var tagExpression = _parser.ParseUnaryConversion<TrackedOperation>("o => \"MyTag\"").Value;
        var conditionExpression = _parser.ParseUnaryPredicate<TrackedOperation>("o => o.Description.Contains(\"test\")").Value;
        var taggingCriterion = new TaggingCriterion(tagExpression, conditionExpression);

        var budget = new TrackedBudget(
            Guid.NewGuid(),
            "Test Budget",
            new[] { owner },
            new[] { taggingCriterion },
            Array.Empty<TransferCriterion>(),
            LogbookCriteria.Universal)
        {
            Version = "v1"
        };

        // Act
        var response = _mapper.ToResponse(budget);

        // Assert
        response.TaggingCriteria.Should().HaveCount(1);
        var tagCriterion = response.TaggingCriteria.First();
        tagCriterion.Tag.Should().Be("o => \"MyTag\"");
        tagCriterion.Condition.Should().Be("o => o.Description.Contains(\"test\")");
        response.LogbookCriteria.Should().HaveCount(1);
    }

    [Fact]
    public void ConvertTransferCriterionToString()
    {
        // Arrange
        var owner = new Owner(Guid.NewGuid(), "Test Owner");
        var transferExpression = _parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(
            "(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1"
        ).Value;
        var transferCriterion = new TransferCriterion(DetectionAccuracy.Likely, "My Transfer", transferExpression);

        var budget = new TrackedBudget(
            Guid.NewGuid(),
            "Test Budget",
            new[] { owner },
            Array.Empty<TaggingCriterion>(),
            new[] { transferCriterion },
            LogbookCriteria.Universal)
        {
            Version = "v1"
        };

        // Act
        var response = _mapper.ToResponse(budget);

        // Assert
        response.TransferCriteria.Should().HaveCount(1);
        var transferResp = response.TransferCriteria.First();
        transferResp.Accuracy.Should().Be("Likely");
        transferResp.Comment.Should().Be("My Transfer");
        transferResp.Criterion.Should().Be("(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1");
    }

    [Fact]
    public void HandleEmptyVersionInResponse()
    {
        // Arrange
        var owner = new Owner(Guid.NewGuid(), "Test Owner");
        var budget = new TrackedBudget(
            Guid.NewGuid(),
            "Test Budget",
            new[] { owner },
            Array.Empty<TaggingCriterion>(),
            Array.Empty<TransferCriterion>(),
            LogbookCriteria.Universal);
        // Version is null

        // Act
        var response = _mapper.ToResponse(budget);

        // Assert
        response.Version.Should().Be(string.Empty);
    }

    [Fact]
    public void ParseValidTaggingCriterionExpressions()
    {
        // Arrange
        var request = new TaggingCriterionResponse(
            "o => \"Shopping\"",
            "o => o.Amount.Amount > 100"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Tag.ToString().Should().Be("o => \"Shopping\"");
        result.Value.Condition.ToString().Should().Be("o => o.Amount.Amount > 100");
    }

    [Fact]
    public void ReturnErrorForInvalidTagExpression()
    {
        // Arrange
        var request = new TaggingCriterionResponse(
            "invalid expression",
            "o => o.Amount.Amount > 100"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("does not match function format");
    }

    [Fact]
    public void ReturnErrorForInvalidConditionExpression()
    {
        // Arrange
        var request = new TaggingCriterionResponse(
            "o => \"Shopping\"",
            "not a valid expression"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("does not match function format");
    }

    [Fact]
    public void ReturnErrorForInvalidPropertyAccessInTaggingCriterion()
    {
        // Arrange
        var request = new TaggingCriterionResponse(
            "o => o.NonExistentProperty",
            "o => o.Amount.Amount > 100"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("Unable to create expression");
    }

    [Fact]
    public void ParseValidTransferCriterionExpression()
    {
        // Arrange
        var request = new TransferCriterionResponse(
            "Exact",
            "My Transfer",
            "(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Accuracy.Should().Be(DetectionAccuracy.Exact);
        result.Value.Comment.Should().Be("My Transfer");
        result.Value.Criterion.ToString().Should().Be("(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1");
    }

    [Fact]
    public void ReturnErrorForInvalidDetectionAccuracy()
    {
        // Arrange
        var request = new TransferCriterionResponse(
            "InvalidAccuracy",
            "My Transfer",
            "(source, sink) => source.Amount.Amount == sink.Amount.Amount * -1"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("Invalid DetectionAccuracy value");
    }

    [Fact]
    public void ReturnErrorForInvalidTransferExpression()
    {
        // Arrange
        var request = new TransferCriterionResponse(
            "Exact",
            "My Transfer",
            "not a valid binary expression"
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("does not match function format");
    }

    [Fact]
    public void ParseUniversalLogbookCriteria()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Universal",
            "Universal",
            null,
            null,
            null,
            null,
            null,
            true
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Description.Should().Be("Universal");
        result.Value.IsUniversal.Should().BeTrue();
    }

    [Fact]
    public void ParseLogbookCriteriaWithSubstitution()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "With Substitution",
            "With Substitution",
            null,
            null,
            null,
            "o => o.Description",
            null,
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Description.Should().Be("With Substitution");
        result.Value.Substitution.Should().NotBeNull();
        result.Value.Substitution!.ToString().Should().Be("o => o.Description");
    }

    [Fact]
    public void ParseLogbookCriteriaWithCriteria()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "With Criteria",
            "With Criteria",
            null,
            null,
            null,
            null,
            "o => o.Amount.Amount > 0",
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Description.Should().Be("With Criteria");
        result.Value.Criteria.Should().NotBeNull();
        result.Value.Criteria!.ToString().Should().Be("o => o.Amount.Amount > 0");
    }

    [Fact]
    public void ParseLogbookCriteriaWithTags()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "With Tags",
            "With Tags",
            null,
            "OneOf",
            new[] { "Tag1", "Tag2" },
            null,
            null,
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Description.Should().Be("With Tags");
        result.Value.Type.Should().Be(Domain.ValueObjects.Criteria.TagBasedCriterionType.OneOf);
        result.Value.Tags.Should().HaveCount(2);
        result.Value.Tags!.Select(t => t.Value).Should().Contain(new[] { "Tag1", "Tag2" });
    }

    [Fact]
    public void ReturnErrorForInvalidTagBasedCriterionType()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Invalid Type",
            "Invalid Type",
            null,
            "InvalidType",
            new[] { "Tag1" },
            null,
            null,
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("Invalid TagBasedCriterionType value");
    }

    [Fact]
    public void ReturnErrorForInvalidSubstitutionExpression()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Invalid Substitution",
            "Invalid Substitution",
            null,
            null,
            null,
            "not a valid expression",
            null,
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("does not match function format");
    }

    [Fact]
    public void ReturnErrorForInvalidCriteriaExpression()
    {
        // Arrange
        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Invalid Criteria",
            "Invalid Criteria",
            null,
            null,
            null,
            null,
            "not a valid predicate",
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Contain("does not match function format");
    }

    [Fact]
    public void ParseRecursiveLogbookSubcriteria()
    {
        // Arrange
        var subCriterion1 = new LogbookCriteriaResponse(Guid.NewGuid(), "Sub1", "Sub1", null, null, null, null, null, true);
        var subCriterion2 = new LogbookCriteriaResponse(Guid.NewGuid(), "Sub2", "Sub2", null, null, null, null, "o => o.Amount.Amount < 0", null);

        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Parent",
            "Parent",
            new[] { subCriterion1, subCriterion2 },
            null,
            null,
            null,
            null,
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeSuccess();
        result.Value.Description.Should().Be("Parent");
        result.Value.Subcriteria.Should().HaveCount(2);
        result.Value.Subcriteria!.ElementAt(0).Description.Should().Be("Sub1");
        result.Value.Subcriteria!.ElementAt(1).Description.Should().Be("Sub2");
    }

    [Fact]
    public void PropagateErrorsFromInvalidSubcriteria()
    {
        // Arrange
        var invalidSubCriterion = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Invalid Sub",
            "Invalid Sub",
            null,
            null,
            null,
            null,
            "invalid expression",
            null
        );

        var request = new LogbookCriteriaResponse(
            Guid.NewGuid(),
            "Parent",
            "Parent",
            new[] { invalidSubCriterion },
            null,
            null,
            null,
            null,
            null
        );

        // Act
        var result = _mapper.FromRequest(request);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void PreserveExpressionsInRoundTripConversion()
    {
        // Arrange
        var originalExpression = "o => o.Description.Contains(\"grocery\")";
        var request = new TaggingCriterionResponse("o => \"Food\"", originalExpression);

        // Act
        var parseResult = _mapper.FromRequest(request);
        var criterion = parseResult.Value;
        
        // Convert back to response
        var owner = new Owner(Guid.NewGuid(), "Test");
        var budget = new TrackedBudget(
            Guid.NewGuid(),
            "Test",
            new[] { owner },
            new[] { criterion },
            Array.Empty<TransferCriterion>(),
            LogbookCriteria.Universal
        ) { Version = "v1" };
        
        var response = _mapper.ToResponse(budget);

        // Assert
        parseResult.Should().BeSuccess();
        response.TaggingCriteria.First().Condition.Should().Be(originalExpression);
    }
}
