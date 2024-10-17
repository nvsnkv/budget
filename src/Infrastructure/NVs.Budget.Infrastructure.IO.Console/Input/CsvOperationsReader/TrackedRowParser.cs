using System.Collections.Concurrent;
using CsvHelper;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.IO.Console.Converters;
using NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader.Errors;
using NVs.Budget.Infrastructure.IO.Console.Input.Errors;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader;

internal class TrackedRowParser(IReader parser, IBudgetsRepository budgetsRepository, CancellationToken cancellationToken) : RowParser<TrackedOperation, CsvTrackedOperation>(parser, cancellationToken)
{
    private readonly ConcurrentDictionary<Guid, Task<TrackedBudget?>> _budgets = new();

    protected override  async Task<Result<TrackedOperation>> Convert(CsvTrackedOperation row)
    {
        var budgetTask = _budgets.GetOrAdd(row.BudgetId, GetBudget);
        var budget = await budgetTask;
        if (budget is null)
        {
            return Result.Fail(new BudgetDoesNotExistError(row.BudgetId));
        }

        if (string.IsNullOrEmpty(row.Amount))
        {
            return Result.Fail(new RowNotParsedError(Row, [new AttributeParsingError(nameof(row.Amount))]));
        }

        Money amount;
        try
        {
            amount = MoneyConverter.Instance.Convert(row.Amount, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(Row, [new AttributeParsingError(nameof(row.Amount)), new ExceptionBasedError(e)]));
        }

        IReadOnlyCollection<Tag> tags;
        try
        {
            tags = TagsConverter.Instance.Convert(row.Tags ?? string.Empty, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(Row, [new AttributeParsingError(nameof(row.Tags)), new ExceptionBasedError(e)]));
        }

        IReadOnlyDictionary<string, object> attributes;
        try
        {
            attributes = AttributesConverter.Instance.Convert(row.Attributes ?? string.Empty, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(Row, [new AttributeParsingError(nameof(row.Attributes)), new ExceptionBasedError(e)]));
        }


        return new TrackedOperation(
            row.Id,
            row.Timestamp,
            amount,
            row.Description ?? string.Empty,
            budget,
            tags,
            attributes
        );
    }

    private async Task<TrackedBudget?> GetBudget(Guid id)
    {
        var budgets = await budgetsRepository.Get(a => a.Id == id, CancellationToken);
        return budgets.FirstOrDefault();
    }
}
