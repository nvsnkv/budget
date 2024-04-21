using CommandLine;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Test;

[Verb("import", HelpText = "Test CsvReadingOptions for particular file")]
internal class TestImportVerb : AbstractVerb
{
    [Option('f', "file", Required = true, HelpText = "Incoming file to test")]
    public string? FilePath { get; set; }
}

internal class TestImportVerbHandler(
    IInputStreamProvider input,
    IOperationsReader reader,
    IOutputStreamProvider output,
    IOptionsSnapshot<OutputOptions> outputOptions,
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

        var successes = new List<Operation>();
        var errors = new List<Result>();

        var ops = reader.ReadUnregisteredOperations(streamResult.Value, filePath, cancellationToken);

        await foreach (var result in ops.WithCancellation(cancellationToken))
        {
            if (result.IsSuccess)
            {
                successes.Add(CreateOperationFrom(result.Value));
            }
            else
            {
                errors.Add(result.ToResult());
            }
        }

        foreach (var error in errors)
        {
            await resultWriter.Write(error, cancellationToken);
        }

        await objectWriter.Write(successes, cancellationToken);

        var writer = await output.GetOutput(outputOptions.Value.OutputStreamName);
        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"Total; {successes.Count + errors.Count}; Successes; {successes.Count}; Errors; {errors.Count}");
        await writer.FlushAsync(cancellationToken);

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
