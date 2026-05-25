using FluentAssertions;
using NVs.Budget.Utilities.Utc;

namespace NVs.Budget.Controllers.Web.Tests;

public class UtcDateTimeNormalizerShould
{
    [Fact]
    public void AsUtcFromApi_KeepsUtc()
    {
        var input = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        input.AsUtcFromApi().Should().Be(input);
    }

    [Fact]
    public void AsUtcFromApi_TreatsUnspecifiedAsUtcWallClock()
    {
        var input = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Unspecified);
        input.AsUtcFromApi().Should().Be(new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void AsUtcFromImport_LocalUsesClientTimeZone()
    {
        var parsed = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Local);
        var utc = parsed.AsUtcFromImport(DateTimeKind.Local, "Europe/Berlin");
        utc.Kind.Should().Be(DateTimeKind.Utc);
        var back = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"));
        back.Hour.Should().Be(14);
        back.Minute.Should().Be(30);
    }

    [Fact]
    public void AsUtcFromImport_UtcLeavesInstant()
    {
        var parsed = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc);
        parsed.AsUtcFromImport(DateTimeKind.Utc, "Europe/Berlin")
            .Should().Be(parsed);
    }

    [Fact]
    public void AsUtcFromImport_UnspecifiedTreatsAsUtc()
    {
        var parsed = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Unspecified);
        parsed.AsUtcFromImport(DateTimeKind.Unspecified, "Europe/Berlin")
            .Should().Be(new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ResolveTimeZone_ThrowsForUnknownId()
    {
        var act = () => "Not/A/Zone".ResolveTimeZone();
        act.Should().Throw<ArgumentException>();
    }
}
