using Microsoft.Extensions.Configuration;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

internal class CriteriaListReader(CriteriaParser criteriaParser, IConfiguration config)
{
    public IReadOnlyList<TransferCriterion> GetTransferCriteria() => config.GetSection("Transfers").GetChildren().Select(GetTransferCriterion).ToList().AsReadOnly();

    private TransferCriterion GetTransferCriterion(IConfigurationSection section) =>
        new(
            section.GetValue<DetectionAccuracy>(nameof(TransferCriterion.Accuracy)),
            section.Key,
            criteriaParser.ParseTransferCriteria(
                section.GetValue<string>(nameof(TransferCriterion.Criterion)) ?? throw new InvalidOperationException("Missing criterion field!")
            ).Compile()
        );
}
