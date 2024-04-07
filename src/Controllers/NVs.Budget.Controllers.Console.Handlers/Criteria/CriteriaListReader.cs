using Microsoft.Extensions.Configuration;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Controllers.Console.Handlers.Criteria;

internal class CriteriaListReader(CriteriaParser parser)
{
    public IReadOnlyList<TransferCriterion> GetTransferCriteria(IConfigurationSection config) => config.GetChildren().Select(GetTransferCriterion).ToList().AsReadOnly();

    public IReadOnlyCollection<TaggingCriterion> GetTaggingCriteria(IConfigurationSection config) => config.GetChildren().Select(GetTaggingCriterion).ToList().AsReadOnly();

    private TaggingCriterion GetTaggingCriterion(IConfigurationSection section)
    {
        return new TaggingCriterion(
            new Tag(section.GetValue<string>(nameof(TaggingCriterion.Tag)) ?? throw new InvalidOperationException("Missing tag field!")),
            parser.ParseTaggingCriteria(
                section.GetValue<string>(nameof(TaggingCriterion.Criterion)) ?? throw new InvalidOperationException("Missing criterion field!")
            ).Compile()
        );
    }

    private TransferCriterion GetTransferCriterion(IConfigurationSection section) =>
        new(
            section.GetValue<DetectionAccuracy>(nameof(TransferCriterion.Accuracy)),
            section.Key,
            parser.ParseTransferCriteria(
                section.GetValue<string>(nameof(TransferCriterion.Criterion)) ?? throw new InvalidOperationException("Missing criterion field!")
            ).Compile()
        );
}
