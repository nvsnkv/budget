using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Web.Utils;

public class LogbookMapper(OperationMapper operationMapper)
{
    public LogbookEntryResponse ToResponse(CriteriaBasedLogbook logbook)
    {
        var sum = logbook.Sum;
        var currency = sum.GetCurrency();
        var children = logbook.Children
            .Select(kvp => ToResponse(kvp.Value))
            .ToList();
        
        // Only include operations if there are no children (leaf node)
        var operations = children.Count == 0
            ? logbook.Operations
                .Cast<Application.Contracts.Entities.Budgeting.TrackedOperation>()
                .Select(operationMapper.ToResponse)
                .ToList()
            : new List<OperationResponse>();

        return new LogbookEntryResponse(
            logbook.Criterion.Description,
            new MoneyResponse(sum.Amount, currency.IsoCode.ToString()),
            logbook.From,
            logbook.Till,
            logbook.Operations.Count(),
            operations,
            children
        );
    }
}

