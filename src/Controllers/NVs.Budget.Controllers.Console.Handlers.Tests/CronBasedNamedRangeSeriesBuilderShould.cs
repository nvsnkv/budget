using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Controllers.Console.Handlers.Utils;

namespace NVs.Budget.Controllers.Console.Handlers.Tests;

public class CronBasedNamedRangeSeriesBuilderShould
{
    [Fact]
    public void GenerateValidSchedule()
    {
        var from = new DateTime(DateTime.Now.Year, 1, 1);
        var till = new DateTime(DateTime.Now.Year + 1, 1, 1);
        var expression = "0 0 1 * *";

        var ranges = new CronBasedNamedRangeSeriesBuilder().GetRanges(from, till, expression);
        ranges.Should().BeSuccess();
        var values = ranges.Value.ToList();
        values.Count.Should().Be(12);
        var i = 1;
        while (i < values.Count)
        {
            var prev = values[i - 1];
            var curr = values[i];

            prev.From.Day.Should().Be(1);
            prev.From.Month.Should().Be(i);
            prev.Till.Should().Be(curr.From);
            i++;
        }

        var last = values.Last();
        last.Till.Should().Be(till);
    }
}
