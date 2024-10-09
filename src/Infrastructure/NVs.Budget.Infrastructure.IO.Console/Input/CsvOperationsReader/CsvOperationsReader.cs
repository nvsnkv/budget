using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader.Errors;
using NVs.Budget.Infrastructure.IO.Console.Input.Options;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader;

internal class CsvOperationsReader(CsvConfiguration configuration, IOptionsSnapshot<CsvReadingOptions> options, IBudgetsRepository budgetsRepository) : IOperationsReader
{
    private readonly CsvReadingOptions _options = options.Value;
    public async IAsyncEnumerable<Result<UnregisteredOperation>> ReadUnregisteredOperations(StreamReader input, string name, [EnumeratorCancellation] CancellationToken ct)
    {
        var fileOptions = _options.GetFileOptionsFor(name);
        if (fileOptions is null)
        {
            yield return Result.Fail(new UnexpectedFileNameGivenError(name));
            yield break;
        }

        var parser = new CsvParser(input, configuration, true);
        var rowParser = new UntrackedRowParser(parser, name, fileOptions, ct);

        while (await rowParser.ReadAsync())
        {
            var validationResult = rowParser.IsRowValid();
            if (validationResult.IsFailed)
            {
                yield return validationResult.ToResult();
            }

            if (validationResult.Value)
            {
                yield return rowParser.GetRow();
            }
        }
    }

    public async IAsyncEnumerable<Result<TrackedOperation>> ReadTrackedOperation(StreamReader input, [EnumeratorCancellation]CancellationToken ct)
    {
        var parser = new CsvReader(input, configuration, true);
        parser.Context.RegisterClassMap<CsvTrackedOperationClassMap>();

        var rowParser = new TrackedRowParser(parser, budgetsRepository, ct);

        while (await rowParser.ReadAsync())
        {
            yield return await rowParser.GetRecord();
        }
    }
}
