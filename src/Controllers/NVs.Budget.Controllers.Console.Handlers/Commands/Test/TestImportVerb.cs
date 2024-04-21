using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Test;

[Verb("import", HelpText = "Test CsvReadingOptions for particular file")]
internal class TestImportVerb : IRequest<ExitCode>
{
    [Option('f', "file", Required = true, HelpText = "Incoming file to test")]
    public string? FilePath { get; set; }
}

internal class TestImportVerbHandler(
    IInputStreamProvider input,
    IOperationsReader reader,
    IResultWriter<Result> resultWriter,
    IObjectWriter<Operation> objectWriter,
    IUser user) : IRequestHandler<TestImportVerb, ExitCode>
{
    public async Task<ExitCode> Handle(TestImportVerb request, CancellationToken cancellationToken)
    {
        var filePath = request.FilePath ?? string.Empty;
        var streamResult = await input.GetInput(filePath);
        if (streamResult.IsFailed)
        {
            await resultWriter.Write(streamResult.ToResult(), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var ops = reader.ReadUnregisteredOperations(streamResult.Value, filePath, cancellationToken)
            .GroupBy(r => r.IsSuccess)
            .OrderBy(g => g.Key);

        await foreach (var group in ops.WithCancellation(cancellationToken))
        {
            if (group.Key == false)
            {
                await foreach (var result in group.WithCancellation(cancellationToken))
                {
                    await resultWriter.Write(result.ToResult(), cancellationToken);
                }
            }
            else
            {
                var validRecords = await group.Select(r => CreateOperationFrom(r.Value)).ToListAsync(cancellationToken);
                await objectWriter.Write(validRecords, cancellationToken);
            }
        }

        return ExitCode.Success;
    }

    private Operation CreateOperationFrom(UnregisteredOperation unregistered)
    {
        return new Operation(
            Guid.Empty,
            unregistered.Timestamp,
            unregistered.Amount,
            unregistered.Description,
            new Account(Guid.Empty, unregistered.Account.Name, unregistered.Account.Bank, [user.AsOwner()]),
            [],
            unregistered.Attributes
        );
    }
}
