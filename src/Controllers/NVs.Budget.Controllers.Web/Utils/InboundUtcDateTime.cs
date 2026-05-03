namespace NVs.Budget.Controllers.Web.Utils;

/// <summary>
/// Converts inbound API timestamps (JSON model binding / query) to UTC for the application layer.
/// DateTime deserialized without offset is <see cref="DateTimeKind.Unspecified"/>; treat as wall clock in the server local zone before converting (see also client <c>datetime-local</c> + UTC ISO payloads).
/// </summary>
public static class InboundUtcDateTime
{
    public static DateTime Normalize(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value.Kind, $"Unexpected {nameof(DateTimeKind)}.")
        };
}
