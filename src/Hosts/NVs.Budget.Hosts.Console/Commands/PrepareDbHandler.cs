using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

namespace NVs.Budget.Hosts.Console.Commands;

[UsedImplicitly]
internal class PrepareDbHandler(IDbMigrator migrator, IResultWriter<Result> writer) : IRequestHandler<PrepareDbVerb, ExitCode>
{
    public async Task<ExitCode> Handle(PrepareDbVerb request, CancellationToken cancellationToken)
    {
        try
        {
            await migrator.MigrateAsync(cancellationToken);
            return ExitCode.Success;
        }
        catch (Exception e)
        {
            await writer.Write(Result.Fail(new ExceptionBasedError(e)), cancellationToken);
            return ExitCode.OperationError;
        }
    }
}
