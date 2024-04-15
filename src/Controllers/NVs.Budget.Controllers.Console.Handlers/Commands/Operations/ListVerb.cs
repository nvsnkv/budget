using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("list", isDefault:true, HelpText = "List operations that match given criteria")]
internal class ListVerb : IRequest<ExitCode>
{
    [Option("currency", HelpText = "Output currency code. If none set, original currencies will be used")]
    public CurrencyIsoCode? CurrencyIsoCode { get; set; }

    [Option("exclude-transfers", HelpText = "Exclude transfers from the list")]
    public bool ExcludeTransfers { get; set; }

    [Value(0, MetaName = "Criteria")]
    public IEnumerable<string>? Criteria { get; set; }
}

internal class ListOperationsVerbHandler(
    IMediator mediator,
    IObjectWriter<TrackedOperation> objectWriter,
    CriteriaParser criteriaParser,
    IResultWriter<Result<Expression<Func<TrackedOperation, bool>>>> resultWriter
) : IRequestHandler<ListVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ListVerb request, CancellationToken cancellationToken)
    {
        var criteria = string.Join(' ', request.Criteria ?? Enumerable.Empty<string>());
        var criteriaResult = criteriaParser.TryParsePredicate<TrackedOperation>(criteria, "o");
        if (!criteriaResult.IsSuccess)
        {
            await resultWriter.Write(criteriaResult, cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var query = new OperationQuery(
            criteriaResult.Value,
            request.CurrencyIsoCode is not null ? Currency.Get(request.CurrencyIsoCode.Value) : null,
            request.ExcludeTransfers
        );

        var operations = mediator.CreateStream(new ListOperationsQuery(query), cancellationToken);

        await objectWriter.Write(await operations.ToListAsync(cancellationToken), cancellationToken);

        return ExitCode.Success;
    }
}
