using FluentAssertions;
using NVs.Budget.Controllers.Web.Utils;

namespace NVs.Budget.Controllers.Web.Tests;

public class InboundUtcDateTimeShould
{
    [Fact]
    public void Utc_round_trips_untouched()
    {
        var u = new DateTime(2026, 4, 1, 14, 30, 0, DateTimeKind.Utc);
        var n = InboundUtcDateTime.Normalize(u);

        n.Should().Be(u);
        n.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Unspecified_behaves_like_local_wall_clock_then_converted_to_utc()
    {
        var v = new DateTime(2030, 11, 5, 9, 15, 0, DateTimeKind.Unspecified);

        var n = InboundUtcDateTime.Normalize(v);

        n.Kind.Should().Be(DateTimeKind.Utc);
        var expected = DateTime.SpecifyKind(v, DateTimeKind.Local).ToUniversalTime();
        n.Should().Be(expected);
    }
}
