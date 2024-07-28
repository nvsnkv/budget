using CsvHelper.Configuration;

namespace NVs.Budget.Controllers.Console.IO.Output.Accounts;

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
