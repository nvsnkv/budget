using System.Globalization;
using FluentResults;
using NCrontab;

namespace NVs.Budget.Utilities.Scheduling;

public static class TimeZoneScheduling
{
    /// <summary>Resolves IANA IDs on modern .NET (e.g. <c>Europe/Moscow</c>). Empty → UTC.</summary>
    public static Result<TimeZoneInfo> ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return Result.Ok(TimeZoneInfo.Utc);
        }

        try
        {
            return Result.Ok(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim()));
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return Result.Fail<TimeZoneInfo>($"Unknown or invalid time zone id: {timeZoneId}");
        }
    }
}

public readonly record struct CronRangeSegment(string Name, DateTime FromUtc, DateTime TillUtc);

/// <summary>
/// Builds half-open partitions [<see cref="CronRangeSegment.FromUtc"/>, <see cref="CronRangeSegment.TillUtc"/>).
/// Cron fields are interpreted in <paramref name="timeZone"/>; output bounds are UTC.
/// </summary>
public static class CronRangePartitioner
{
    /// <summary>Formats <paramref name="fromUtc"/>–<paramref name="tillUtc"/> as calendar dates in <paramref name="timeZone"/>.</summary>
    public static string FormatWallCombinedRange(DateTime fromUtc, DateTime tillUtc, TimeZoneInfo timeZone) =>
        FormatCombinedWallRangeLabel(NormalizeToUtc(fromUtc), NormalizeToUtc(tillUtc), timeZone);

    /// <summary>Column label for a period start instant in <paramref name="timeZone"/> (same rules as cron segments).</summary>
    public static string FormatWallRangePeriodStart(DateTime segmentStartUtc, TimeSpan spanHintAcrossPlan, TimeZoneInfo timeZone) =>
        FormatSegmentStartWallLabel(NormalizeToUtc(segmentStartUtc), spanHintAcrossPlan, timeZone);

    public static Result<IReadOnlyList<CronRangeSegment>> GetSegments(
        DateTime from,
        DateTime till,
        string? cronExpression,
        TimeZoneInfo timeZone)
    {
        var fromUtc = NormalizeToUtc(from);
        var tillUtc = NormalizeToUtc(till);
        if (tillUtc < fromUtc)
        {
            return Result.Fail<IReadOnlyList<CronRangeSegment>>("Till date must be after From date");
        }

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            var name = FormatCombinedWallRangeLabel(fromUtc, tillUtc, timeZone);
            return Result.Ok<IReadOnlyList<CronRangeSegment>>(new[] { new CronRangeSegment(name, fromUtc, tillUtc) });
        }

        CrontabSchedule schedule;
        try
        {
            schedule = CrontabSchedule.Parse(cronExpression);
        }
        catch (Exception e)
        {
            return Result.Fail<IReadOnlyList<CronRangeSegment>>($"Invalid cron expression: {e.Message}");
        }

        var fromWall = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, timeZone);
        var tillWall = TimeZoneInfo.ConvertTimeFromUtc(tillUtc, timeZone);

        var rawCronWall = schedule
            .GetNextOccurrences(ToUnspecifiedWall(fromWall).AddDays(-1), ToUnspecifiedWall(tillWall).AddDays(1))
            .Select(ToUnspecifiedWall)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var interiorUtcSorted = new List<DateTime>();
        foreach (var wall in rawCronWall)
        {
            DateTime utc;
            try
            {
                utc = TimeZoneInfo.ConvertTimeToUtc(wall, timeZone);
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (utc <= fromUtc || utc >= tillUtc)
            {
                continue;
            }

            if (interiorUtcSorted.Count == 0 || interiorUtcSorted[^1] != utc)
            {
                interiorUtcSorted.Add(utc);
            }
        }

        var boundariesUtc = new List<DateTime> { fromUtc };
        foreach (var utc in interiorUtcSorted)
        {
            if (boundariesUtc[^1] == utc)
            {
                continue;
            }

            boundariesUtc.Add(utc);
        }

        if (boundariesUtc[^1] != tillUtc)
        {
            boundariesUtc.Add(tillUtc);
        }

        if (boundariesUtc.Count < 2)
        {
            return Result.Fail<IReadOnlyList<CronRangeSegment>>("Cron partitioning produced an invalid boundary list");
        }

        var span = boundariesUtc[^1] - boundariesUtc[0];
        var segments = new List<CronRangeSegment>();
        for (var i = 1; i < boundariesUtc.Count; i++)
        {
            var segFrom = boundariesUtc[i - 1];
            var segTill = boundariesUtc[i];
            var label = FormatSegmentStartWallLabel(segFrom, span, timeZone);
            segments.Add(new CronRangeSegment(label, segFrom, segTill));
        }

        return Result.Ok<IReadOnlyList<CronRangeSegment>>(segments);
    }

    private static DateTime NormalizeToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();

    /// <summary>Strip zone kind so NCrontab and <see cref="TimeZoneInfo.ConvertTimeToUtc"/> treat values as zone wall time.</summary>
    private static DateTime ToUnspecifiedWall(DateTime d) =>
        new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, DateTimeKind.Unspecified);

    private static string FormatSegmentStartWallLabel(DateTime segmentStartUtc, TimeSpan wholeSpan, TimeZoneInfo tz)
    {
        var wall = TimeZoneInfo.ConvertTimeFromUtc(segmentStartUtc, tz);
        var format = wholeSpan > TimeSpan.FromDays(365) ? "dd'/'MM'/'yy" : "dd'/'MM";
        return wall.ToString(format, CultureInfo.InvariantCulture);
    }

    private static string FormatCombinedWallRangeLabel(DateTime fromUtc, DateTime tillUtc, TimeZoneInfo tz)
    {
        var span = tillUtc - fromUtc;
        var format = span > TimeSpan.FromDays(365) ? "dd'/'MM'/'yy" : "dd'/'MM";
        var fromWall = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, tz);
        var tillWall = TimeZoneInfo.ConvertTimeFromUtc(tillUtc, tz);
        return $"{fromWall.ToString(format, CultureInfo.InvariantCulture)} - {tillWall.ToString(format, CultureInfo.InvariantCulture)}";
    }
}
