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

public record TaggingCriterionResponse(
    string Tag,
    string Condition
);

public record TransferCriterionResponse(
    string Accuracy,
    string Comment,
    string Criterion
);

public record LogbookCriteriaResponse(
    string Description,
    IReadOnlyCollection<LogbookCriteriaResponse>? Subcriteria,
    string? Type,
    IReadOnlyCollection<string>? Tags,
    string? Substitution,
    string? Criteria,
    bool? IsUniversal
);
