using System.Collections.Concurrent;
using CsvHelper;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Controllers.Console.IO.Converters;
using NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader.Errors;
using NVs.Budget.Controllers.Console.IO.Models;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader;

internal class TrackedRowParser(IReader parser, IAccountsRepository accountsRepository, CancellationToken ct)
{
    private readonly ConcurrentDictionary<Guid, Task<TrackedAccount?>> _accounts = new();
    private volatile int _row = -1;

    public async Task<bool> ReadAsync()
    {
        ct.ThrowIfCancellationRequested();

        var result = await parser.ReadAsync();
        if (result)
        {
            Interlocked.Increment(ref _row);
        }

        return result;
    }

    public async Task<Result<TrackedOperation>> GetRecord()
    {
        CsvTrackedOperation csvRow;
        try
        {
            csvRow = parser.GetRecord<CsvTrackedOperation>();
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(_row, [new ExceptionBasedError(e)]));
        }

        var accountTask = _accounts.GetOrAdd(csvRow.AccountId, GetAccount);
        var account = await accountTask;
        if (account is null)
        {
            return Result.Fail(new AccountDoesNotExistError(csvRow.AccountId));
        }

        if (string.IsNullOrEmpty(csvRow.Amount))
        {
            return Result.Fail(new RowNotParsedError(_row, [new AttributeParsingError(nameof(csvRow.Amount))]));
        }

        Money amount;
        try
        {
            amount = MoneyConverter.Instance.Convert(csvRow.Amount, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(_row, [new AttributeParsingError(nameof(csvRow.Amount)), new ExceptionBasedError(e)]));
        }

        IReadOnlyCollection<Tag> tags;
        try
        {
            tags = TagsConverter.Instance.Convert(csvRow.Tags ?? string.Empty, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(_row, [new AttributeParsingError(nameof(csvRow.Tags)), new ExceptionBasedError(e)]));
        }

        IReadOnlyDictionary<string, object> attributes;
        try
        {
            attributes = AttributesConverter.Instance.Convert(csvRow.Attributes ?? string.Empty, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(_row, [new AttributeParsingError(nameof(csvRow.Attributes)), new ExceptionBasedError(e)]));
        }


        return new TrackedOperation(
            csvRow.Id,
            csvRow.Timestamp,
            amount,
            csvRow.Description ?? string.Empty,
            account,
            tags,
            attributes
        );
    }

    private async Task<TrackedAccount?> GetAccount(Guid id)
    {
        var accounts = await accountsRepository.Get(a => a.Id == id, ct);
        return accounts.FirstOrDefault();
    }
}
