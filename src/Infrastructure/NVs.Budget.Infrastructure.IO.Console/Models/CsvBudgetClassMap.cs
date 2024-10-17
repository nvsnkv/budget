using CsvHelper.Configuration;

namespace NVs.Budget.Infrastructure.IO.Console.Models;

internal class CsvBudgetClassMap : ClassMap<CsvBudget>
{
    public CsvBudgetClassMap()
    {
        Map(a => a.Id);
        Map(a => a.Name);
        Map(a => a.Owners);
    }
}
