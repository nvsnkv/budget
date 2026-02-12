using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredLogbookCriteria
{
    private static readonly Guid DefaultCriteriaId = Guid.Empty;
    public static readonly StoredLogbookCriteria Universal = new()
    {
        CriteriaId = DefaultCriteriaId,
        Name = "Default"
    };

    public Guid CriteriaId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IList<StoredLogbookCriteria>? Subcriteria { get; init; }
    public TagBasedCriterionType? Type { get; init; }
    public IList<StoredTag>? Tags { get; init; }
    public string? Substitution { get; init; }
    public string? Criteria { get; init; }
    public bool? IsUniversal { get; init; }
}
