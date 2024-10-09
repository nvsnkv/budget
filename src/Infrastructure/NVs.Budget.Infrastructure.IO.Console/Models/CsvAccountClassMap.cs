using CsvHelper.Configuration;

namespace NVs.Budget.Infrastructure.IO.Console.Models;

internal class CsvAccountClassMap : ClassMap<CsvAccount>
{
    public CsvAccountClassMap()
    {
        Map(a => a.Id);
        Map(a => a.Name);
        Map(a => a.Bank);
        Map(a => a.Owners);
    }
}
