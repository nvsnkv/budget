using AutoMapper;
using ClosedXML.Excel;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Logbook;

internal class LogbookOperationsWriter(IXLWorksheet worksheet, IMapper mapper)
{
    private int _rowNum = 1;
    private int _colNum = 1;

    public void WriteValue(CriteriaBasedLogbook logbook, NamedRange range)
    {

        if (!logbook[range.From, range.Till].Operations.Any())
        {
            return;
        }

        worksheet.Cell(_rowNum, _colNum).SetValue(logbook.Criterion.Description);
        worksheet.Cell(_rowNum, _colNum + 1).SetValue(range.Name);
        worksheet.Cell(_rowNum, _colNum + 2).SetValue(range.From);
        worksheet.Cell(_rowNum, _colNum + 3).SetValue(range.Till);
        _rowNum++;

        var logbooks = logbook.Children;
        if (logbooks.Any())
        {
            _colNum++;
            if (logbook.Criterion is SubstitutionBasedCriterion)
            {
                logbooks = logbooks.OrderBy(c => c.Key.Description).ToDictionary();
            }

            foreach (var (_, child) in logbooks)
            {
                WriteValue(child, range);
            }

            _colNum--;
        }
        else
        {
            worksheet.Cell(_rowNum, _colNum).SetValue("|");
            worksheet.Cell(_rowNum, _colNum + 1).SetValue(nameof(CsvTrackedOperation.Id));
            worksheet.Cell(_rowNum, _colNum + 2).SetValue(nameof(CsvTrackedOperation.Timestamp));
            worksheet.Cell(_rowNum, _colNum + 3).SetValue(nameof(CsvTrackedOperation.Amount));
            worksheet.Cell(_rowNum, _colNum + 4).SetValue(nameof(CsvTrackedOperation.Description));
            worksheet.Cell(_rowNum, _colNum + 5).SetValue(nameof(CsvTrackedOperation.Version));
            worksheet.Cell(_rowNum, _colNum + 6).SetValue(nameof(CsvTrackedOperation.Tags));
            worksheet.Cell(_rowNum, _colNum + 7).SetValue(nameof(CsvTrackedOperation.Attributes));
            worksheet.Cell(_rowNum, _colNum + 8).SetValue(nameof(CsvTrackedOperation.BudgetId));
            worksheet.Cell(_rowNum, _colNum + 9).SetValue(nameof(CsvTrackedOperation.Budget));
            worksheet.Cell(_rowNum, _colNum + 10).SetValue(nameof(CsvTrackedOperation.Bank));
            _rowNum++;

            foreach (var operation in logbook[range.From, range.Till].Operations.Select(mapper.Map<CsvTrackedOperation>))
            {
                worksheet.Cell(_rowNum, _colNum).SetValue("|");
                worksheet.Cell(_rowNum, _colNum + 1).SetValue(operation.Id.ToString());
                worksheet.Cell(_rowNum, _colNum + 2).SetValue(operation.Timestamp);
                worksheet.Cell(_rowNum, _colNum + 3).SetValue(operation.Amount);
                worksheet.Cell(_rowNum, _colNum + 4).SetValue(operation.Description);
                worksheet.Cell(_rowNum, _colNum + 5).SetValue(operation.Version);
                worksheet.Cell(_rowNum, _colNum + 6).SetValue(operation.Tags);
                worksheet.Cell(_rowNum, _colNum + 7).SetValue(operation.Attributes);
                worksheet.Cell(_rowNum, _colNum + 8).SetValue(operation.BudgetId.ToString());
                worksheet.Cell(_rowNum, _colNum + 9).SetValue(operation.Budget);
                worksheet.Cell(_rowNum, _colNum + 10).SetValue(operation.Bank);

                _rowNum++;
            }
        }
    }
}
