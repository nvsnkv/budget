using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedBudget : Domain.Entities.Budgets.Budget, ITrackableEntity<Guid>
{
    private readonly List<TaggingCriterion> _taggingCriteria;
    private readonly List<TransferCriterion> _transferCriteria;
    private readonly List<LogbookCriteria> _logbookCriteria;

    public TrackedBudget(
        Guid id,
        string name,
        IEnumerable<Owner> owners,
        IEnumerable<TaggingCriterion> taggingCriteria,
        IEnumerable<TransferCriterion> transferCriteria,
        IReadOnlyCollection<LogbookCriteria> logbookCriteria)
        : base(id, name, owners)
    {
        _taggingCriteria = [..Order(taggingCriteria)];
        _transferCriteria = [..Order(transferCriteria)];
        _logbookCriteria = [..NormalizeLogbookCriteria(logbookCriteria)];
    }

    public TrackedBudget(
        Guid id,
        string name,
        IEnumerable<Owner> owners,
        IEnumerable<TaggingCriterion> taggingCriteria,
        IEnumerable<TransferCriterion> transferCriteria,
        LogbookCriteria logbookCriteria)
        : this(id, name, owners, taggingCriteria, transferCriteria, [logbookCriteria])
    {
    }

    public IReadOnlyCollection<TaggingCriterion> TaggingCriteria => _taggingCriteria.AsReadOnly();
    public IReadOnlyList<TransferCriterion> TransferCriteria => _transferCriteria.AsReadOnly();
    public IReadOnlyCollection<LogbookCriteria> LogbookCriteria => _logbookCriteria.AsReadOnly();
    public string? Version { get; set; }

    public void SetTaggingCriteria(IEnumerable<TaggingCriterion> criteria)
    {
        _taggingCriteria.Clear();
        _taggingCriteria.AddRange(Order(criteria));
    }

    public void SetTransferCriteria(IEnumerable<TransferCriterion> criteria)
    {
        _transferCriteria.Clear();
        _transferCriteria.AddRange(Order(criteria));
    }

    public void SetLogbookCriteria(LogbookCriteria value)
    {
        SetLogbookCriteria([value]);
    }

    public void SetLogbookCriteria(IReadOnlyCollection<LogbookCriteria> values)
    {
        _logbookCriteria.Clear();
        _logbookCriteria.AddRange(NormalizeLogbookCriteria(values));
    }

    private static IEnumerable<TaggingCriterion> Order(IEnumerable<TaggingCriterion> criteria) => criteria.OrderBy(r => r.Tag.ToString()).ThenBy(r => r.Condition.ToString());
    private static IEnumerable<TransferCriterion> Order(IEnumerable<TransferCriterion> criteria) => criteria.OrderBy(c => c.Comment);

    private static IEnumerable<LogbookCriteria> NormalizeLogbookCriteria(IReadOnlyCollection<LogbookCriteria> criteriaCollection)
    {
        if (criteriaCollection.Count == 0)
        {
            return [NVs.Budget.Application.Contracts.Criteria.LogbookCriteria.Universal];
        }

        var normalized = criteriaCollection
            .Where(c => c.CriteriaId != Guid.Empty && !string.IsNullOrWhiteSpace(c.Name))
            .GroupBy(c => c.CriteriaId)
            .Select(g => g.First())
            .ToList();

        return normalized.Count == 0
            ? [NVs.Budget.Application.Contracts.Criteria.LogbookCriteria.Universal]
            : normalized;
    }
}
