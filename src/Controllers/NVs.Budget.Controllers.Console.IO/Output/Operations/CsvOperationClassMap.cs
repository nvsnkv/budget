using CsvHelper.Configuration;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal sealed class CsvOperationClassMap : ClassMap<CsvOperation>
{
    public CsvOperationClassMap()
    {
        Map(m => m.Timestamp).Index(0);
        Map(m => m.Amount).Index(1);
        Map(m => m.Description).Index(2);
        Map(m => m.Tags).Index(4);
        Map(m => m.Attributes).Index(5);
        Map(m => m.Account).Index(6);
        Map(m => m.Bank).Index(7);
    }
}