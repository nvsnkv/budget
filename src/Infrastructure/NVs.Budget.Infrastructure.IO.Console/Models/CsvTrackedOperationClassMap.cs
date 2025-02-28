using CsvHelper.Configuration;

namespace NVs.Budget.Infrastructure.IO.Console.Models;

internal sealed class CsvTrackedOperationClassMap : ClassMap<CsvTrackedOperation>
{
    public CsvTrackedOperationClassMap()
    {
        Map(m => m.Id).Index(0);
        Map(m => m.Timestamp).Index(1);
        Map(m => m.Amount).Index(2);
        Map(m => m.Description).Index(3);
        Map(m => m.Version).Index(4);
        Map(m => m.Tags).Index(5);
        Map(m => m.Attributes).Index(6);
        Map(m => m.BudgetId).Index(7);
        Map(m => m.Budget).Index(8);
    }
}
