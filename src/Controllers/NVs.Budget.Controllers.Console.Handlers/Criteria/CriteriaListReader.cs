using Microsoft.Extensions.Configuration;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Controllers.Console.Handlers.Criteria;

internal class CriteriaListReader(CriteriaParser criteriaParser, SubstitutionsParser substitutionsParser, IConfiguration config)
{
    public IReadOnlyList<TransferCriterion> GetTransferCriteria() => config.GetSection("Transfers").GetChildren().Select(GetTransferCriterion).ToList().AsReadOnly();

    public IReadOnlyCollection<TaggingCriterion> GetTaggingCriteria() => config.GetSection("Tags").GetChildren().SelectMany(GetTaggingCriterion).ToList().AsReadOnly();

    private IEnumerable<TaggingCriterion> GetTaggingCriterion(IConfigurationSection section)
    {
        var tagFn = substitutionsParser.GetSubstitutions<TrackedOperation>(section.Key, "o");

        var criteria = section.Get<string[]>() ?? [];
        foreach (var criterion in criteria)
        {
            yield return new TaggingCriterion(o => new Tag(tagFn(o)), criteriaParser.ParseTaggingCriteria(criterion).Compile());
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
