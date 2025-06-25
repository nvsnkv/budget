using AutoMapper;
using ClosedXML.Excel;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Logbook;

internal class CriteriaBasedXLLogbook
{
    private readonly CriteriaBasedLogbook _source;
    private readonly XLWorkbook _workbook;

    private readonly IEnumerable<NamedRange> _ranges;
    private readonly OperationCountsWriter? _countSheet;
    private readonly AmountsWriter? _amountsSheet;
    private readonly LogbookOperationsWriter? _operationsSheet;


    public CriteriaBasedXLLogbook(CriteriaBasedLogbook logbook, LogbookWritingOptions options, IMapper mapper)
    {
        _source = logbook;
        _ranges = options.Ranges;
        _workbook = new XLWorkbook();

        _amountsSheet = new AmountsWriter(_workbook.AddWorksheet("Amounts"));

        if (options.WriteCounts)
        {
            _countSheet = new OperationCountsWriter(_workbook.AddWorksheet("Counts"));
        }

        if (options.WriteOperations)
        {
            _operationsSheet = new LogbookOperationsWriter(_workbook.AddWorksheet("Operations"), mapper);
        }
    }

    public void SaveTo(Stream stream)
    {
        _countSheet?.ResetPosition();
        _amountsSheet?.ResetPosition();
        _countSheet?.WriteCriteriaNames(_source.Children);
        _amountsSheet?.WriteCriteriaNames(_source.Children);

        foreach (var range in _ranges)
        {
            _countSheet?.WriteRangeName(range);
            _amountsSheet?.WriteRangeName(range);
            foreach (var (_, logbook) in _source.Children)
            {
                _countSheet?.WriteValue(logbook, range);
                _amountsSheet?.WriteValue(logbook, range);
                _operationsSheet?.WriteValue(logbook, range);
            }

            _countSheet?.NextCol();
            _amountsSheet?.NextCol();
        }

        _workbook.SaveAs(stream);
    }
}
