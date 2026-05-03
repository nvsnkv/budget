using FluentAssertions;
using NVs.Budget.Application.Services.Accounting.Plans;

namespace NVs.Budget.Application.Tests;

public class BudgetPlanRangeBuilderShould
{
    [Fact]
    public void UseDayMonthYearOrderForLongRangeNames()
    {
        var builder = new BudgetPlanRangeBuilder();

        var result = builder.GetRanges(
            new DateTime(2026, 1, 15),
            new DateTime(2027, 2, 16),
            "0 0 15 * *");

        result.IsSuccess.Should().BeTrue();
        result.Value.First().Name.Should().Be("15/01/26");
    }
}
