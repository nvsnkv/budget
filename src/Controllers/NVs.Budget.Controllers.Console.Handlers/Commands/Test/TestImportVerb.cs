using CommandLine;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

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

        await foreach (var result in ops)
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
            new Domain.Entities.Accounts.Budget(Guid.Empty, "fake budget", [user.AsOwner()]),
            [],
            unregistered.Attributes
        );
    }
}
