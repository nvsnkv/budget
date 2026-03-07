using FluentAssertions;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Tests;

public class DemoBudgetGeneratorShould
{
    private readonly DemoBudgetGenerator _generator = new(ReadableExpressionsParser.Default);

    [Fact]
    public void GenerateSeedWithExpectedCriteriaAndOperationsRange()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var seed = _generator.Generate(budgetId, now);

        // Assert
        seed.TaggingCriteria.Should().Contain(c => c.Tag.ToString().Contains("Withdraws"));
        seed.TaggingCriteria.Should().Contain(c => c.Tag.ToString().Contains("Expenses"));

        seed.LogbookCriteria.Should().HaveCount(1);
        seed.LogbookCriteria.Single().Description.Should().Be("Demo");

        seed.Operations.Count.Should().BeGreaterOrEqualTo(200);
        seed.Operations.Count.Should().BeLessOrEqualTo(500);

        var from = now.AddMonths(-6);
        seed.Operations.Should().OnlyContain(o => o.Timestamp >= from && o.Timestamp <= now);
    }

    [Fact]
    public void IncludeCategoryAttributeForExpenseOperations()
    {
        // Arrange
        var budgetId = Guid.NewGuid();

        // Act
        var seed = _generator.Generate(budgetId, DateTime.UtcNow);

        // Assert
        var expenses = seed.Operations.Where(o => o.Amount.Amount < 0).ToList();
        expenses.Should().NotBeEmpty();
        expenses.Should().OnlyContain(o =>
            o.Attributes != null &&
            o.Attributes.ContainsKey("category") &&
            o.Attributes["category"] is string);
    }
}
