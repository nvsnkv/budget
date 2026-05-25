namespace NVs.Budget.Utilities.Utc;

public static class UtcDateTimeNormalizer
{
    public static DateTime AsUtcFromApi(this DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    public static DateTime AsUtcFromLocalWallClock(this DateTime wallClock, string ianaTimeZoneId)
    {
        var timeZone = ianaTimeZoneId.ResolveTimeZone();
        var unspecified = DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }

    public static DateTime AsUtcFromImport(this DateTime parsed, DateTimeKind kind, string clientIanaTimeZoneId) =>
        kind switch
        {
            DateTimeKind.Local => parsed.AsUtcFromLocalWallClock(clientIanaTimeZoneId),
            DateTimeKind.Utc => DateTime.SpecifyKind(parsed, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
        };

    public static DateTime AsUtcExclusiveTillFromCalendarDate(this string yyyyMmDd, string ianaTimeZoneId)
    {
        if (!DateOnly.TryParse(yyyyMmDd, out var date))
        {
            throw new ArgumentException($"Invalid calendar date: {yyyyMmDd}", nameof(yyyyMmDd));
        }

        var nextDay = date.AddDays(1);
        return nextDay.ToDateTime(TimeOnly.MinValue).AsUtcFromLocalWallClock(ianaTimeZoneId);
    }

    public static TimeZoneInfo ResolveTimeZone(this string ianaTimeZoneId)
    {
        if (string.IsNullOrWhiteSpace(ianaTimeZoneId))
        {
            throw new ArgumentException("Time zone id is required.", nameof(ianaTimeZoneId));
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZoneId.Trim());
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new ArgumentException($"Unknown time zone: {ianaTimeZoneId}", nameof(ianaTimeZoneId), ex);
        }
        catch (InvalidTimeZoneException ex)
        {
            throw new ArgumentException($"Invalid time zone: {ianaTimeZoneId}", nameof(ianaTimeZoneId), ex);
        }
    }
}
