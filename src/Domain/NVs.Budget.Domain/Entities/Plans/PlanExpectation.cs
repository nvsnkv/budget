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
        string? subcriterionName = null,
        string? note = null) : base(id)
    {
        if (till < from)
        {
            throw new ArgumentException("Till date must be greater than or equal to From date.", nameof(till));
        }

        ExpectedAmount = expectedAmount;
        From = from;
        Till = till;
        SubcriterionName = string.IsNullOrWhiteSpace(subcriterionName) ? null : subcriterionName.Trim();
        Note = string.IsNullOrWhiteSpace(note) ? null : note;
    }

    public Money ExpectedAmount { get; }

    public DateTime From { get; }

    public DateTime Till { get; }

    public string? SubcriterionName { get; }

    public string? Note { get; }
}
