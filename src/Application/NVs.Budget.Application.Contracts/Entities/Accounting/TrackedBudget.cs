using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedBudget(Guid id, string name, IEnumerable<Owner> owners, IEnumerable<TaggingCriterion> taggingCriteria)
    : Domain.Entities.Accounts.Budget(id, name, owners), ITrackableEntity<Guid>
{
    private readonly List<TaggingCriterion> _taggingRules = [..OrderCriteria(taggingCriteria)];

    public IReadOnlyCollection<TaggingCriterion> TaggingCriteria => _taggingRules.AsReadOnly();
    public string? Version { get; set; }

    public void SetTaggingRules(IEnumerable<TaggingCriterion> criteria)
    {
        _taggingRules.Clear();
        _taggingRules.AddRange(OrderCriteria(criteria));
    }

    private static IEnumerable<TaggingCriterion> OrderCriteria(IEnumerable<TaggingCriterion> criteria) => criteria.OrderBy(r => r.Tag).ThenBy(r => r.Condition);
}
