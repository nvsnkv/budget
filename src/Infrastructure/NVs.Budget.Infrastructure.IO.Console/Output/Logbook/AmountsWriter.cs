using ClosedXML.Excel;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Logbook;

internal class AmountsWriter(IXLWorksheet worksheet) : OperationCountsWriter(worksheet)
{
    protected override void SetValue(IXLCell xlCell, Domain.Aggregates.Logbook logbook)
    {
        var value = logbook.Sum.Amount;
        if (value == decimal.Zero)
        {
            return;
        }

        xlCell.SetValue(value);
        xlCell.Style.NumberFormat.SetNumberFormatId(2);
    }
}
