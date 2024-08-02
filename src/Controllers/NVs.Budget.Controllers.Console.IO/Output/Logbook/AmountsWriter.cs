using ClosedXML.Excel;

namespace NVs.Budget.Controllers.Console.IO.Output.Logbook;

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
