using System.Collections.Concurrent;
using CsvHelper;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Controllers.Console.IO.Converters;
using NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader.Errors;
using NVs.Budget.Controllers.Console.IO.Input.Errors;
using NVs.Budget.Controllers.Console.IO.Models;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader;

internal class TrackedRowParser(IReader parser, IAccountsRepository accountsRepository, CancellationToken cancellationToken) : RowParser<TrackedOperation, CsvTrackedOperation>(parser, cancellationToken)
{
    private readonly ConcurrentDictionary<Guid, Task<TrackedAccount?>> _accounts = new();

    protected override  async Task<Result<TrackedOperation>> Convert(CsvTrackedOperation row)
    {
        var accountTask = _accounts.GetOrAdd(row.AccountId, GetAccount);
        var account = await accountTask;
        if (account is null)
        {
            return Result.Fail(new AccountDoesNotExistError(row.AccountId));
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
            account,
            tags,
            attributes
        );
    }

    private async Task<TrackedAccount?> GetAccount(Guid id)
    {
        var accounts = await accountsRepository.Get(a => a.Id == id, CancellationToken);
        return accounts.FirstOrDefault();
    }
}
