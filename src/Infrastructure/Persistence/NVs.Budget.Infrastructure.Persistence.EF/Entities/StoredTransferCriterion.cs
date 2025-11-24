using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredTransferCriterion
{
    public DetectionAccuracy Accuracy { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Criterion { get; set; } = string.Empty;

    public virtual StoredBudget Budget { get; set; } = StoredBudget.Invalid;
}
