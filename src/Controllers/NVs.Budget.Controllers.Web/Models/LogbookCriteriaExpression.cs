using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Controllers.Web.Models;

public class LogbookCriteriaExpression
{
    public IDictionary<string, LogbookCriteriaExpression>? Subcriteria { get; set; }
    public TagBasedCriterionType? Type { get; set; }
    public IEnumerable<string>? Tags { get; set; }
    public string? Substitution { get; set; }
    public string? Criteria { get; set; }
}