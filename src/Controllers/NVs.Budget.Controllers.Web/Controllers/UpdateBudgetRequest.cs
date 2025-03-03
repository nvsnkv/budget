namespace NVs.Budget.Controllers.Web.Controllers;

public record UpdateBudgetRequest(
    Guid Id,
    string Name,
    string Version,
    string TaggingCriteria,
    string TransferCriteria,
    string LogbookCriteria
);