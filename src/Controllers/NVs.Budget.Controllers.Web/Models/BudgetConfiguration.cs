namespace NVs.Budget.Controllers.Web.Models;

public class BudgetConfiguration
{
    public Guid Id { get; init; }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public IDictionary<string, IEnumerable<string>>? Tags { get; set; }

    public IDictionary<string, IEnumerable<TransferCriterionExpression>>? Transfers { get; set; }

    public IDictionary<string, LogbookCriteriaExpression>? Logbook { get; set; }
}
