using CsvHelper.Configuration;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal sealed class CsvTransferClassMap : ClassMap<CsvTransfer>
{
    public CsvTransferClassMap()
    {
        Map(m => m.SourceId).Index(0);
        Map(m => m.SinkId).Index(1);
        Map(m => m.Fee).Index(2);
        Map(m => m.Accuracy).Index(3);
        Map(m => m.Comment).Index(4);

        Map(m => m.Source).Ignore();
        Map(m => m.Sink).Ignore();
    }
}