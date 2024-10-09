using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("list-duplicates", isDefault: false, HelpText = "List duplicated operations that matches criteria")]
internal class ListDuplicatesVerb : CriteriaBasedVerb;

internal class ListDuplicatesVerbHandler(IMediator mediator,
    IObjectWriter<TrackedOperation> objectWriter,
    ICriteriaParser criteriaParser,
    IResultWriter<Result> resultWriter
) : CriteriaBasedVerbHandler<ListDuplicatesVerb, TrackedOperation>(criteriaParser, resultWriter)
{
    protected override async Task<ExitCode> HandleInternal(ListDuplicatesVerb request, Expression<Func<TrackedOperation, bool>> criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDuplicatedOperationsQuery(criteria), cancellationToken);
        await objectWriter.Write(result.SelectMany(r => r), cancellationToken);

        return ExitCode.Success;
    }
}
