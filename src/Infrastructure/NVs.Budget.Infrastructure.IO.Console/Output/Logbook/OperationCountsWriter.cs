using ClosedXML.Excel;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Logbook;

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

    public void WriteCriteriaNames(IReadOnlyDictionary<Criterion, CriteriaBasedLogbook> children)
    {
        if (!children.Any())
        {
            return;
        }

        WriteCriteriaNames(children, 1);

        _colNum = _criteriaDepth + 1;
    }

    private void WriteCriteriaNames(IReadOnlyDictionary<Criterion, CriteriaBasedLogbook> children, int offset)
    {
        _criteriaDepth = offset > _criteriaDepth ? offset : _criteriaDepth;
        foreach (var (criterion, logbook) in children.OrderBy(c => c.Key.Description))
        {
            if (criterion is UniversalCriterion && string.IsNullOrEmpty(criterion.Description) && logbook.IsEmpty)
            {
                continue;
            }

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
        if (logbook is { IsEmpty: true, Criterion: UniversalCriterion } && string.IsNullOrEmpty(logbook.Criterion.Description))
        {
            return;
        }

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
        if (value == 0)
        {
            return;
        }

        xlCell.SetValue(value);
        xlCell.Style.NumberFormat.SetNumberFormatId(1);
    }

    public void NextCol()
    {
        _colNum++;
    }
}
