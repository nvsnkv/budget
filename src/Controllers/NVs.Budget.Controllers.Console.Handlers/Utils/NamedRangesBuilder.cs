using FluentResults;
using NCrontab;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Controllers.Console.Handlers.Utils;

internal class CronBasedNamedRangeSeriesBuilder
{
    public Result<IEnumerable<NamedRange>> GetRanges(DateTime from, DateTime till, string cronExpr)
    {
        if (till < from)
        {
            return Result.Fail(new IncorrectDateRangeGivenError());
        }

        CrontabSchedule schedule;
        try
        {
            schedule = CrontabSchedule.Parse(cronExpr);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionBasedError(e));
        }

        var occurences = schedule.GetNextOccurrences(from.AddDays(-1), till.AddDays(1)).OrderBy(d => d).ToList();
        if (occurences.Count < 2)
        {
            return Result.Fail(new EmptyRangeGivenError());
        }

        return Result.Ok(GenerateRangesFrom(occurences));
    }

    private IEnumerable<NamedRange> GenerateRangesFrom(List<DateTime> occurences)
    {
        var i = 1;
        while (i < occurences.Count)
        {
            yield return new NamedRange(occurences[i - 1].ToString("dd/MM"), occurences[i - 1], occurences[i]);
            i++;
        }
    }
}
