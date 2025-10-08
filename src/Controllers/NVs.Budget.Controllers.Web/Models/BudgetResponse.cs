using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Controllers.Web.Models;

public record BudgetResponse(
    Guid Id,
    string Name,
    string Version,
    IReadOnlyCollection<Owner> Owners,
    IReadOnlyCollection<TaggingCriterionResponse> TaggingCriteria,
    IReadOnlyCollection<TransferCriterionResponse> TransferCriteria,
    LogbookCriteriaResponse LogbookCriteria
);

public class TaggingCriterionResponse
{
    public string Tag { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
}

public class TransferCriterionResponse
{
    public string Accuracy { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string Criterion { get; set; } = string.Empty;
}

public class LogbookCriteriaResponse
{
    public string Description { get; set; } = string.Empty;
    public IReadOnlyCollection<LogbookCriteriaResponse>? Subcriteria { get; set; }
    public string? Type { get; set; }
    public IReadOnlyCollection<string>? Tags { get; set; }
    public string? Substitution { get; set; }
    public string? Criteria { get; set; }
    public bool? IsUniversal { get; set; }
}
