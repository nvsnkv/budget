using AutoMapper;
using ClosedXML.Excel;
using NMoneys;
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
        worksheet.Cell(_rowNum, _colNum + 4).SetValue(logbook.Criterion.Description);

        var ranged = logbook[range.From, range.Till];

        var sumCell = worksheet.Cell(_rowNum, _colNum + 4);
        sumCell.SetValue(ranged.Sum.Amount);
        sumCell.Style.NumberFormat.Format = "# ###0,00\" \"[$\u20bd-419]";

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
            _rowNum++;

            foreach (var operation in ranged.Operations.Cast<TrackedOperation>())
            {
                var converted = mapper.Map<CsvTrackedOperation>(operation);

                worksheet.Cell(_rowNum, _colNum).SetValue("|");
                worksheet.Cell(_rowNum, _colNum + 1).SetValue(operation.Id.ToString());
                var timestampCell = worksheet.Cell(_rowNum, _colNum + 2);
                timestampCell.Style.NumberFormat.Format = "yyyy-MM-dd HH:mm:ss";
                timestampCell.SetValue(operation.Timestamp);

                var amountCell = worksheet.Cell(_rowNum, _colNum + 3);
                amountCell.SetValue(operation.Amount.Amount);
                amountCell.Style.NumberFormat.Format = "# ###0,00\" \"[$\u20bd-419]";
                worksheet.Cell(_rowNum, _colNum + 4).SetValue(operation.Description);
                worksheet.Cell(_rowNum, _colNum + 5).SetValue(operation.Version);
                worksheet.Cell(_rowNum, _colNum + 6).SetValue(converted.Tags);
                worksheet.Cell(_rowNum, _colNum + 7).SetValue(converted.Attributes);
                worksheet.Cell(_rowNum, _colNum + 8).SetValue(converted.BudgetId.ToString());
                worksheet.Cell(_rowNum, _colNum + 9).SetValue(converted.Budget);

                _rowNum++;
            }
        }
    }
}
