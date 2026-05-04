using NMoneys;
using NVs.Budget.Domain.Entities;

namespace NVs.Budget.Domain.Entities.Plans;

public class PlanExpectation : EntityBase<Guid>
{
    public PlanExpectation(
        Guid id,
        Money expectedAmount,
        DateTime from,
        DateTime till,
        string? subcriterionPath = null,
        string? note = null) : base(id)
    {
        var utcFrom = ToUtc(from);
        var utcTill = ToUtc(till);

        if (utcTill < utcFrom)
        {
            throw new ArgumentException("Till date must be greater than or equal to From date.", nameof(till));
        }

        ExpectedAmount = expectedAmount;
        From = utcFrom;
        Till = utcTill;
        SubcriterionPath = string.IsNullOrWhiteSpace(subcriterionPath) ? null : subcriterionPath.Trim();
        Note = string.IsNullOrWhiteSpace(note) ? null : note;
    }

    public Money ExpectedAmount { get; }

    public DateTime From { get; }

    public DateTime Till { get; }

    /// <summary>
    /// Path of the targeted sub-criterion under this plan's root criterion,
    /// built from descriptions joined with '/' (same convention as the logbook UI).
    /// Single-segment values without '/' are treated as legacy leaf descriptions.
    /// </summary>
    public string? SubcriterionPath { get; }

    public string? Note { get; }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
