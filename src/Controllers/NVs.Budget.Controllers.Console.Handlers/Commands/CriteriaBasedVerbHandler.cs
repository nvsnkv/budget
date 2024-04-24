using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

internal class CriteriaBasedVerb : AbstractVerb
{
    [Value(0, MetaName = "Criteria")]
    public IEnumerable<string>? Criteria { get; set; }
}

internal abstract class CriteriaBasedVerbHandler<T, TPredicate>(CriteriaParser parser, IResultWriter<Result> writer) : IRequestHandler<T, ExitCode> where T: CriteriaBasedVerb
{
    protected readonly IResultWriter<Result> Writer = writer;

    public async Task<ExitCode> Handle(T request, CancellationToken cancellationToken)
    {
        var criteria = string.Join(' ', request.Criteria ?? Enumerable.Empty<string>());
        var criteriaResult = parser.TryParsePredicate<TPredicate>(criteria, "o");
        if (!criteriaResult.IsSuccess)
        {
            await Writer.Write(criteriaResult.ToResult(), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        return await HandleInternal(request, criteriaResult.Value, cancellationToken);
    }

    protected abstract Task<ExitCode> HandleInternal(T request, Expression<Func<TPredicate, bool>> criteriaResultValue, CancellationToken cancellationToken);
}
