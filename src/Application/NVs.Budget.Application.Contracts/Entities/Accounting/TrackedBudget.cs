using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedBudget(
    Guid id,
    string name,
    IEnumerable<Owner> owners,
    IEnumerable<TaggingCriterion> taggingCriteria,
    IEnumerable<TransferCriterion> transferCriteria,
    IEnumerable<LogbookCriteria> logbookCriteria)
    : Domain.Entities.Budgets.Budget(id, name, owners), ITrackableEntity<Guid>
{
    private readonly List<TaggingCriterion> _taggingCriteria = [..Order(taggingCriteria)];
    private readonly List<TransferCriterion> _transferCriteria = [..Order(transferCriteria)];
    private readonly List<LogbookCriteria> _logbookCriteria = [..Order(ValidateAndNormalize(logbookCriteria))];

    public IReadOnlyCollection<TaggingCriterion> TaggingCriteria => _taggingCriteria.AsReadOnly();
    public IReadOnlyList<TransferCriterion> TransferCriteria => _transferCriteria.AsReadOnly();
    public IReadOnlyList<LogbookCriteria> LogbookCriteria => _logbookCriteria.AsReadOnly();
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

    public void SetLogbookCriteria(IEnumerable<LogbookCriteria> values)
    {
        _logbookCriteria.Clear();
        _logbookCriteria.AddRange(Order(ValidateAndNormalize(values)));
    }

    private static IEnumerable<TaggingCriterion> Order(IEnumerable<TaggingCriterion> criteria) => criteria.OrderBy(r => r.Tag.ToString()).ThenBy(r => r.Condition.ToString());
    private static IEnumerable<TransferCriterion> Order(IEnumerable<TransferCriterion> criteria) => criteria.OrderBy(c => c.Comment);
    private static IEnumerable<LogbookCriteria> Order(IEnumerable<LogbookCriteria> criteria) => criteria.OrderBy(c => c.Description, StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyCollection<LogbookCriteria> ValidateAndNormalize(IEnumerable<LogbookCriteria> criteria)
    {
        var criteriaList = (criteria ?? []).ToList();
        if (criteriaList.Count == 0)
        {
            return [Criteria.LogbookCriteria.Universal];
        }

        var duplicateNames = criteriaList
            .GroupBy(c => c.Description.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Count != 0)
        {
            throw new ArgumentException($"Duplicate LogbookCriteria descriptions are not allowed: {string.Join(", ", duplicateNames)}", nameof(criteria));
        }

        return criteriaList;
    }
}
