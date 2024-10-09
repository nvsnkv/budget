using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("import", HelpText = "Import operations from files")]
internal class ImportVerb : AbstractVerb
{
    [Option('d', "dir", Required = true, HelpText = "Directory with files to import")]
    public string? DirectoryPath { get; [UsedImplicitly] set; }

    [Option("register-accs", Required = false, HelpText = "Create new accounts if no existing accounts matches imported data"), Obsolete("TODO REMOVE OPTION")]
    public bool RegisterAccounts { get; [UsedImplicitly] set; }

    [Option("transfers-confidence", Required = false, Default = DetectionAccuracy.Exact, HelpText = "Transfers detection accuracy (Exact or Likely)")]
    public DetectionAccuracy DetectionAccuracy { get; [UsedImplicitly] set; }
}

internal class ImportVerbHandler(
    IInputStreamProvider input,
    IResultWriter<Result> resultWriter,
    IOperationsReader reader,
    IMediator mediator
) : IRequestHandler<ImportVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ImportVerb request, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(request.DirectoryPath))
        {
            return ExitCode.ArgumentsError;
        }

        var exitCodes = new HashSet<ExitCode> { ExitCode.Success };

        var options = new ImportOptions(request.RegisterAccounts, request.DetectionAccuracy);

        foreach (var file in Directory.EnumerateFiles(request.DirectoryPath))
        {
            var fileStreamResult = await input.GetInput(file);
            if (fileStreamResult.IsFailed)
            {
                await resultWriter.Write(fileStreamResult.ToResult(), cancellationToken);
                exitCodes.Add(ExitCode.ArgumentsError);
            }
            else
            {
                var operations = reader.ReadUnregisteredOperations(fileStreamResult.Value, file, cancellationToken);
                var parsedOperations = operations.SelectAwait(async r =>
                {
                    if (r.IsSuccess) return r.Value;
                    await resultWriter.Write(r.ToResult(), cancellationToken);
                    exitCodes.Add(r.ToExitCode());
                    return null;

                }).Where(o => o is not null);

                var result = await mediator.Send(new ImportOperationsCommand(parsedOperations!, options), cancellationToken);
                exitCodes.Add(result.ToExitCode());
            }
        }

        return exitCodes.Aggregate((r, e) => r | e);
    }
}
