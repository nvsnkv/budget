using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Controllers.Console.IO.Output.Logbook;

internal class CriteriaBasedXLLogbook
{
    private readonly CriteriaBasedLogbook _source;
    private readonly XLWorkbook _workbook;

    private readonly IEnumerable<NamedRange> _ranges;
    private readonly WorksheetWriter? _countSheet;


    public CriteriaBasedXLLogbook(CriteriaBasedLogbook logbook, Stream stream, LogbookWritingOptions options)
    {
        _source = logbook;
        _ranges = options.Ranges;
        _workbook = new XLWorkbook(stream);

        if (options.WriteCounts)
        {
            _countSheet = new WorksheetWriter(_workbook.AddWorksheet("Counts"));
        }
    }

    public void Save()
    {
        _countSheet?.ResetPosition();
        _countSheet?.WriteCriteriaNames(_source.Children);
        _countSheet?.ShiftCol();
        foreach (var range in _ranges)
        {
            _countSheet?.WriteRangeName(range);
            foreach (var (_, logbook) in _source.Children)
            {
                _countSheet?.WriteValue(logbook, range);
            }

            _countSheet?.NextCol();
        }
    }
}

internal class WorksheetWriter (IXLWorksheet worksheet)
{
    private readonly Dictionary<Criterion, int> _criterionPositions = new();

    private int _criteriaDepth;
    private int _rowNum;
    private int _colNum;

    public void ResetPosition()
    {
        _rowNum = _colNum = 0;
    }

    public void WriteCriteriaNames(IReadOnlyDictionary<Criterion, CriteriaBasedLogbook> children, int offset = 0)
    {
        _colNum = offset;
        _criteriaDepth = offset + 1 > _criteriaDepth ? offset + 1 : _criteriaDepth;
        foreach (var (criterion, logbook) in children)
        {
            WriteCriterion(criterion);
            WriteCriteriaNames(logbook.Children, offset + 1);
        }
    }

    private void WriteCriterion(Criterion criterion)
    {
        worksheet.Cell(_rowNum, _colNum).SetValue(criterion.Description);
        _criterionPositions.Add(criterion, _rowNum);
        _rowNum++;
    }

    public void WriteRangeName(NamedRange range)
    {
        var xlCell = worksheet.Cell(0, _colNum);
        xlCell.SetValue(range.Name);
        xlCell.Style.Alignment.SetTextRotation(90);
    }

    public void WriteValue(CriteriaBasedLogbook logbook, NamedRange range)
    {
        var value = logbook[range.From, range.Till].Operations.Count();
        var rowNum = _criterionPositions[logbook.Criterion];

        var xlCell = worksheet.Cell(rowNum, _colNum);
        xlCell.SetValue(value);
        foreach (var (_, child) in logbook.Children)
        {
            WriteValue(child, range);
        }
    }

    public void ShiftCol()
    {
        _colNum = _criteriaDepth + 1;
    }

    public void NextCol()
    {
        _colNum++;
    }
}
