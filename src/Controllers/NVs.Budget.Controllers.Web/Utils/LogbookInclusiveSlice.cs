namespace NVs.Budget.Controllers.Web.Utils;

internal static class LogbookInclusiveSlice
{
    /// <summary>
    /// Cron segments are logical [<paramref name="from"/>, <paramref name="exclusiveTill"/>);
    /// <see cref="NVs.Budget.Domain.Aggregates.Logbook"/> uses an inclusive upper bound.
    /// </summary>
    public static DateTime ToInclusiveTill(DateTime from, DateTime exclusiveTill) =>
        exclusiveTill > from ? exclusiveTill.AddTicks(-1) : exclusiveTill;
}
