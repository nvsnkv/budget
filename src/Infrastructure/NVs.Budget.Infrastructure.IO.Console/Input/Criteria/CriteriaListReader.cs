using Microsoft.Extensions.Configuration;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

internal class CriteriaListReader(CriteriaParser criteriaParser, IConfiguration config)
{
    public IReadOnlyList<TransferCriterion> GetTransferCriteria() => config.GetSection("Transfers").GetChildren().Select(GetTransferCriterion).ToList().AsReadOnly();

    public IReadOnlyCollection<TaggingRule> GetTaggingCriteria() => config.GetSection("Tags").GetChildren().SelectMany(GetTaggingCriterion).ToList().AsReadOnly();

    private IEnumerable<TaggingRule> GetTaggingCriterion(IConfigurationSection section)
    {
        var criteria = section.Get<string[]>() ?? [];
        foreach (var criterion in criteria)
        {
            yield return new TaggingRule(section.Key, criterion);
        }
    }

    private TransferCriterion GetTransferCriterion(IConfigurationSection section) =>
        new(
            section.GetValue<DetectionAccuracy>(nameof(TransferCriterion.Accuracy)),
            section.Key,
            criteriaParser.ParseTransferCriteria(
                section.GetValue<string>(nameof(TransferCriterion.Criterion)) ?? throw new InvalidOperationException("Missing criterion field!")
            ).Compile()
        );
}
