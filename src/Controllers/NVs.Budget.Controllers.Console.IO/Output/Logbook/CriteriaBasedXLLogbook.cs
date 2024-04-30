using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Console.IO.Output.Logbook;

internal class CriteriaBasedXLLogbook
{
    private readonly CriteriaBasedLogbook _source;
    private readonly XLWorkbook _workbook;

    private readonly IEnumerable<NamedRange> _ranges;
    private readonly OperationCountsWriter? _countSheet;
    private readonly AmountsWriter? _amountsSheet;


    public CriteriaBasedXLLogbook(CriteriaBasedLogbook logbook, LogbookWritingOptions options)
    {
        _source = logbook;
        _ranges = options.Ranges;
        _workbook = new XLWorkbook();

        if (options.WriteCounts)
        {
            _countSheet = new OperationCountsWriter(_workbook.AddWorksheet("Counts"));
        }

        if (options.WriteAmounts)
        {
            _amountsSheet = new AmountsWriter(_workbook.AddWorksheet("Amounts"));
        }
    }

    public void SaveTo(Stream stream)
    {
        _countSheet?.ResetPosition();
        _amountsSheet?.ResetPosition();
        _countSheet?.WriteCriteriaNames(_source.Children);
        _amountsSheet?.WriteCriteriaNames(_source.Children);
        _countSheet?.ShiftCol();
        _amountsSheet?.ShiftCol();

        foreach (var range in _ranges)
        {
            _countSheet?.WriteRangeName(range);
            _amountsSheet?.WriteRangeName(range);
            foreach (var (_, logbook) in _source.Children)
            {
                _countSheet?.WriteValue(logbook, range);
                _amountsSheet?.WriteValue(logbook, range);
            }

            _countSheet?.NextCol();
            _amountsSheet?.NextCol();
        }

        _workbook.SaveAs(stream);
    }
}
