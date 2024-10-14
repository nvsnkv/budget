using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvOperationsReader;

internal class CsvOperationsReader(CsvConfiguration configuration, IBudgetsRepository budgetsRepository) : IOperationsReader
{
    public async IAsyncEnumerable<Result<UnregisteredOperation>> ReadUnregisteredOperations(StreamReader input, SpecificCsvFileReadingOptions fileOptions, [EnumeratorCancellation] CancellationToken ct)
    {
        var parser = new CsvParser(input, configuration, true);
        var rowParser = new UntrackedRowParser(parser, fileOptions, ct);

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
