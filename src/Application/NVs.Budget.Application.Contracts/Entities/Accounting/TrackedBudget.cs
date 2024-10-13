using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedBudget(Guid id, string name, IEnumerable<Owner> owners, IEnumerable<TaggingCriterion> taggingCriteria, IEnumerable<TransferCriterion> transferCriteria)
    : Domain.Entities.Accounts.Budget(id, name, owners), ITrackableEntity<Guid>
{
    private readonly List<TaggingCriterion> _taggingCriteria = [..Order(taggingCriteria)];
    private readonly List<TransferCriterion> _transferCriteria = [..Order(transferCriteria)];

    public IReadOnlyCollection<TaggingCriterion> TaggingCriteria => _taggingCriteria.AsReadOnly();
    public IReadOnlyList<TransferCriterion> TransferCriteria => _transferCriteria.AsReadOnly();
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

    private static IEnumerable<TaggingCriterion> Order(IEnumerable<TaggingCriterion> criteria) => criteria.OrderBy(r => r.Tag.ToString()).ThenBy(r => r.Condition.ToString());
    private static IEnumerable<TransferCriterion> Order(IEnumerable<TransferCriterion> criteria) => criteria.OrderBy(c => c.Comment);
}
