using FluentResults;
using NCrontab;
using NVs.Budget.Domain.Aggregates.Plans;

namespace NVs.Budget.Application.Services.Accounting.Plans;

internal class BudgetPlanRangeBuilder
{
    public Result<IReadOnlyCollection<PlanRange>> GetRanges(DateTime from, DateTime till, string? cronExpression)
    {
        if (till < from)
        {
            return Result.Fail("Till date must be after From date");
        }

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return Result.Ok<IReadOnlyCollection<PlanRange>>([new PlanRange($"{from:dd'/'MM} - {till:dd'/'MM}", from, till)]);
        }

        CrontabSchedule schedule;
        try
        {
            schedule = CrontabSchedule.Parse(cronExpression);
        }
        catch (Exception e)
        {
            return Result.Fail($"Invalid cron expression: {e.Message}");
        }

        var occurrences = schedule.GetNextOccurrences(from.AddDays(-1), till.AddDays(1))
            .OrderBy(d => d)
            .ToList();

        if (occurrences.Count < 2)
        {
            return Result.Fail("Cron expression must generate at least 2 occurrences within the date range");
        }

        return Result.Ok<IReadOnlyCollection<PlanRange>>(GenerateRangesFrom(occurrences).ToList());
    }

    private static IEnumerable<PlanRange> GenerateRangesFrom(List<DateTime> occurrences)
    {
        var least = occurrences.First();
        var last = occurrences.Last();
        var format = (last - least) > TimeSpan.FromDays(365) ? "dd'/'MM'/'yy" : "dd'/'MM";

        var i = 1;
        while (i < occurrences.Count)
        {
            yield return new PlanRange(
                occurrences[i - 1].ToString(format),
                occurrences[i - 1],
                occurrences[i]
            );
            i++;
        }
    }
}
