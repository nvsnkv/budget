using FluentResults;
using NVs.Budget.Utilities.Scheduling;

namespace NVs.Budget.Controllers.Web.Utils;

public class RangeBuilder
{
    public Result<IEnumerable<NamedRange>> GetRanges(
        DateTime from,
        DateTime till,
        string? cronExpr,
        TimeZoneInfo timeZone)
    {
        var segments = CronRangePartitioner.GetSegments(from, till, cronExpr, timeZone);
        if (segments.IsFailed)
        {
            return Result.Fail<IEnumerable<NamedRange>>(segments.Errors);
        }

        return Result.Ok(segments.Value.Select(s => new NamedRange(s.Name, s.FromUtc, s.TillUtc)));
    }
}

public record NamedRange(string Name, DateTime From, DateTime Till);
