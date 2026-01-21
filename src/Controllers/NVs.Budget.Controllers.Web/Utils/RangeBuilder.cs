using FluentResults;
using NCrontab;

namespace NVs.Budget.Controllers.Web.Utils;

public class RangeBuilder
{
    public Result<IEnumerable<NamedRange>> GetRanges(DateTime from, DateTime till, string? cronExpr)
    {
        if (till < from)
        {
            return Result.Fail("Till date must be after From date");
        }

        // If no cron expression, return single range
        if (string.IsNullOrWhiteSpace(cronExpr))
        {
            var name = $"{from:dd/MM} - {till:dd/MM}";
            return Result.Ok<IEnumerable<NamedRange>>(new[] { new NamedRange(name, from, till) });
        }

        CrontabSchedule schedule;
        try
        {
            schedule = CrontabSchedule.Parse(cronExpr);
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

        return Result.Ok(GenerateRangesFrom(occurrences));
    }

    private IEnumerable<NamedRange> GenerateRangesFrom(List<DateTime> occurrences)
    {
        var i = 1;
        while (i < occurrences.Count)
        {
            yield return new NamedRange(
                occurrences[i - 1].ToString("dd/MM"),
                occurrences[i - 1],
                occurrences[i]
            );
            i++;
        }
    }
}

public record NamedRange(string Name, DateTime From, DateTime Till);

