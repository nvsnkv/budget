using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.IO.Output.Operations;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvTransfersReader;

internal class CsvTransfersReader(CsvConfiguration configuration, IOperationsRepository repository) : ITransfersReader
{
    public async IAsyncEnumerable<Result<UnregisteredTransfer>> ReadUnregisteredTransfers(StreamReader input, [EnumeratorCancellation] CancellationToken ct)
    {
        var parser = new CsvReader(input, configuration, true);
        parser.Context.RegisterClassMap<CsvTransferClassMap>();

        var reader = new TransferRowReader(parser, ct, repository);

        while (await reader.ReadAsync())
        {
            yield return await reader.GetRecord();
        }
    }
}
