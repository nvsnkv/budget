using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("list", isDefault:true, HelpText = "List operations that match given criteria")]
internal class ListVerb : CriteriaBasedVerb
{
    [Option("currency", HelpText = "Output currency code. If none set, original currencies will be used")]
    public CurrencyIsoCode? CurrencyIsoCode { get; set; }

    [Option("exclude-transfers", HelpText = "Exclude transfers from the list")]
    public bool ExcludeTransfers { get; set; }
}

internal class ListVerbHandler(
    IMediator mediator,
    IObjectWriter<TrackedOperation> objectWriter,
    ICriteriaParser criteriaParser,
    IResultWriter<Result> resultWriter
) : CriteriaBasedVerbHandler<ListVerb, TrackedOperation>(criteriaParser, resultWriter)
{
    protected override async Task<ExitCode> HandleInternal(ListVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var query = new OperationQuery(
            criteriaResultValue,
            request.CurrencyIsoCode is not null ? Currency.Get(request.CurrencyIsoCode.Value) : null,
            request.ExcludeTransfers
        );

        var operations = mediator.CreateStream(new ListOperationsQuery(query), cancellationToken);

        await objectWriter.Write(await operations.ToListAsync(cancellationToken), cancellationToken);

        return ExitCode.Success;
    }
}
