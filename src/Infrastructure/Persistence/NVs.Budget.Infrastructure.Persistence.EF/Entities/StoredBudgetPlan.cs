using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredBudgetPlan : DbRecord, ITrackableEntity<Guid>
{
    [Key]
    public Guid Id { get; [UsedImplicitly] private set; }

    public Guid BudgetId { get; set; }

    public virtual StoredBudget Budget { get; set; } = StoredBudget.Invalid;

    public string Name { get; set; } = string.Empty;

    public DateTime From { get; set; }

    public DateTime Till { get; set; }

    public string? CronExpression { get; set; }

    public CurrencyIsoCode CurrencyCode { get; set; }

    public decimal ExpectedAmount { get; set; }

    public string? Note { get; set; }

    public StoredLogbookCriteria LogbookCriteria { get; set; } = StoredLogbookCriteria.Universal;

    public IList<StoredPlanExpectation> Expectations { get; set; } = new List<StoredPlanExpectation>();

    public string? Version { get; set; } = string.Empty;
}
