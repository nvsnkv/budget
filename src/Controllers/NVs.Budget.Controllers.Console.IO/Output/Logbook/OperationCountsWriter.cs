using ClosedXML.Excel;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Controllers.Console.IO.Output.Logbook;

internal class OperationCountsWriter (IXLWorksheet worksheet)
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
        var rowNum = _criterionPositions[logbook.Criterion];
        var xlCell = worksheet.Cell(rowNum, _colNum);

        SetValue(xlCell, logbook[range.From, range.Till]);

        foreach (var (_, child) in logbook.Children)
        {
            WriteValue(child, range);
        }
    }

    protected virtual void SetValue(IXLCell xlCell, Domain.Aggregates.Logbook logbook)
    {
        var value = logbook.Operations.Count();
        xlCell.SetValue(value);
        xlCell.Style.NumberFormat.SetNumberFormatId(1);
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
