using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedBudget(Guid id, string name, IEnumerable<Owner> owners, IEnumerable<TaggingRule> taggingRules)
    : Domain.Entities.Accounts.Budget(id, name, owners), ITrackableEntity<Guid>
{
    private readonly List<TaggingRule> _taggingRules = [..OrderRules(taggingRules)];

    public IReadOnlyCollection<TaggingRule> TaggingRules => _taggingRules.AsReadOnly();
    public string? Version { get; set; }

    public void SetTaggingRules(IEnumerable<TaggingRule> rules)
    {
        _taggingRules.Clear();
        _taggingRules.AddRange(OrderRules(rules));
    }

    private static IEnumerable<TaggingRule> OrderRules(IEnumerable<TaggingRule> rules) => rules.OrderBy(r => r.Tag).ThenBy(r => r.Condition);
}
