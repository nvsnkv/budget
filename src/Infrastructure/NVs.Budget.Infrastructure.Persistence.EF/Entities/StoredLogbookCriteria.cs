using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredLogbookCriteria
{
    public static readonly StoredLogbookCriteria Universal = new();

    public string Description { get; init; } = string.Empty;
    public IList<StoredLogbookCriteria>? Subcriteria { get; init; }
    public TagBasedCriterionType? Type { get; init; }
    public IList<StoredTag>? Tags { get; init; }
    public string? Substitution { get; init; }
    public string? Criteria { get; init; }


}
