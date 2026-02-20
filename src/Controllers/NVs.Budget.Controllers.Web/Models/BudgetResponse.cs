using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Controllers.Web.Models;

public record BudgetResponse(
    Guid Id,
    string Name,
    string Version,
    IReadOnlyCollection<Owner> Owners,
    IReadOnlyCollection<TaggingCriterionResponse> TaggingCriteria,
    IReadOnlyCollection<TransferCriterionResponse> TransferCriteria,
    IReadOnlyCollection<LogbookCriteriaResponse> LogbookCriteria
);

public class TaggingCriterionResponse
{
    public string Tag { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    
    // Constructor for backward compatibility with tests
    public TaggingCriterionResponse() { }
    public TaggingCriterionResponse(string tag, string condition)
    {
        Tag = tag;
        Condition = condition;
    }
}

public class TransferCriterionResponse
{
    public string Accuracy { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string Criterion { get; set; } = string.Empty;
    
    // Constructor for backward compatibility with tests
    public TransferCriterionResponse() { }
    public TransferCriterionResponse(string accuracy, string comment, string criterion)
    {
        Accuracy = accuracy;
        Comment = comment;
        Criterion = criterion;
    }
}

public class LogbookCriteriaResponse
{
    public string Description { get; set; } = string.Empty;
    public List<LogbookCriteriaResponse>? Subcriteria { get; set; }
    public string? Type { get; set; }
    public List<string>? Tags { get; set; }
    public string? Substitution { get; set; }
    public string? Criteria { get; set; }
    public bool? IsUniversal { get; set; }
    
    // Constructor for backward compatibility with tests
    public LogbookCriteriaResponse() { }
    public LogbookCriteriaResponse(
        string description,
        IReadOnlyCollection<LogbookCriteriaResponse>? subcriteria,
        string? type,
        IReadOnlyCollection<string>? tags,
        string? substitution,
        string? criteria,
        bool? isUniversal)
    {
        Description = description;
        Subcriteria = subcriteria?.ToList();
        Type = type;
        Tags = tags?.ToList();
        Substitution = substitution;
        Criteria = criteria;
        IsUniversal = isUniversal;
    }
}

public class UpdateBudgetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<TaggingCriterionResponse>? TaggingCriteria { get; set; }
    public List<TransferCriterionResponse>? TransferCriteria { get; set; }
    public List<LogbookCriteriaResponse>? LogbookCriteria { get; set; }
}
