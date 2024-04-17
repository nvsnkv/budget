using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("list-duplicates", isDefault:false, HelpText = "List duplicated operations that matches criteria")]
internal class ListDuplicatesVerb : IRequest<ExitCode>
{
    [Value(0, MetaName = "Criteria")]
    public IEnumerable<string>? Criteria { get; set; }
}

internal class ListDuplicatesVerbHandler(IMediator mediator,
    IObjectWriter<TrackedOperation> objectWriter,
    CriteriaParser criteriaParser,
    IResultWriter<Result<Expression<Func<TrackedOperation, bool>>>> resultWriter) : IRequestHandler<ListDuplicatesVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ListDuplicatesVerb request, CancellationToken cancellationToken)
    {
        var criteria = string.Join(' ', request.Criteria ?? Enumerable.Empty<string>());
        var criteriaResult = criteriaParser.TryParsePredicate<TrackedOperation>(criteria, "o");
        if (!criteriaResult.IsSuccess)
        {
            await resultWriter.Write(criteriaResult, cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var result = await mediator.Send(new ListDuplicatedOperationsQuery(criteriaResult.Value), cancellationToken);
        await objectWriter.Write(result.SelectMany(r => r), cancellationToken);

        return ExitCode.Success;
    }
}
