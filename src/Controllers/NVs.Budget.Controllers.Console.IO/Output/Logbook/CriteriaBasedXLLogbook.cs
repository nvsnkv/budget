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


    public CriteriaBasedXLLogbook(CriteriaBasedLogbook logbook, LogbookWritingOptions options)
    {
        _source = logbook;
        _ranges = options.Ranges;
        _workbook = new XLWorkbook();

        if (options.WriteCounts)
        {
            _countSheet = new WorksheetWriter(_workbook.AddWorksheet("Counts"));
        }
    }

    public void SaveTo(Stream stream)
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

        _workbook.SaveAs(stream);
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
        _rowNum = 2; _colNum = 1;
    }

    public void WriteCriteriaNames(IReadOnlyDictionary<Criterion, CriteriaBasedLogbook> children, int offset = 1)
    {
        if (!children.Any())
        {
            return;
        }

        _criteriaDepth = offset > _criteriaDepth ? offset : _criteriaDepth;
        foreach (var (criterion, logbook) in children)
        {
            _colNum = offset;
            WriteCriterion(criterion);
            WriteCriteriaNames(logbook.Children, offset + 1);
        }
    }

    private void WriteCriterion(Criterion criterion)
    {
        var xlCell = worksheet.Cell(_rowNum, _colNum);
        xlCell.SetValue(criterion.Description);

        _criterionPositions.Add(criterion, _rowNum);
        _rowNum++;
    }

    public void WriteRangeName(NamedRange range)
    {
        var xlCell = worksheet.Cell(1, _colNum);
        xlCell.SetValue(range.Name);
        xlCell.Style.Alignment.SetTextRotation(90);
    }

    public void WriteValue(CriteriaBasedLogbook logbook, NamedRange range)
    {
        var value = logbook[range.From, range.Till].Operations.Count();
        var rowNum = _criterionPositions[logbook.Criterion];

        var xlCell = worksheet.Cell(rowNum, _colNum);
        xlCell.SetValue(value);
        xlCell.Style.NumberFormat.SetNumberFormatId(1);

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
