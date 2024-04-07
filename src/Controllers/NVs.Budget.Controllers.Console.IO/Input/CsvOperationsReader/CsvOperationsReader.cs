using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.IO.Input.Errors;
using NVs.Budget.Controllers.Console.IO.Input.Options;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader;

internal class CsvOperationsReader(CsvConfiguration configuration, IOptions<CsvReadingOptions> options) : IOperationsReader
{
    private readonly CsvReadingOptions _options = options.Value;
    public async IAsyncEnumerable<Result<UnregisteredOperation>> ReadUnregisteredOperations(Stream input, string name, [EnumeratorCancellation] CancellationToken ct)
    {
        var fileOptions = _options.GetFileOptionsFor(name);
        if (fileOptions is null)
        {
            yield return Result.Fail(new UnexpectedFileNameGivenError(name));
            yield break;
        }

        using var reader = new StreamReader(input);
        var parser = new CsvParser(reader, configuration);
        var rowParser = new RowParser(parser, name, fileOptions, ct);

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
}
