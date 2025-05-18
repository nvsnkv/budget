using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Controllers.Web.Models;

public class TransferCriterionExpression
{
    public DetectionAccuracy Accuracy { get; set; }
    public IEnumerable<string>? Criteria { get; set; }
}