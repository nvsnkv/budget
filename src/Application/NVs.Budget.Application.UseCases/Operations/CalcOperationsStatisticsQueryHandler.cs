using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Application.UseCases.Operations;

internal class CalcOperationsStatisticsQueryHandler(IReckoner reckoner) : IRequestHandler<CalcOperationsStatisticsQuery, Result<CriteriaBasedLogbook>>
{
    public async Task<Result<CriteriaBasedLogbook>> Handle(CalcOperationsStatisticsQuery request, CancellationToken cancellationToken)
    {
        var reasons = new List<IReason>();

        var logbookQuery = new LogbookQuery(
            request.Criterion,
            request.OutputCurrency,
            request.OperationsFilter,
            request.ExcludeTransfers
        );

        var operations = reckoner.GetOperations(logbookQuery, cancellationToken);
        var logbook = new CriteriaBasedLogbook(logbookQuery.LogbookCriterion);

        await foreach (var operation in operations)
        {
            var result = logbook.Register(operation);
            if (!result.IsSuccess)
            {
                reasons.AddRange(result.Reasons);
            }
        }

        return Result.Ok(logbook).WithReasons(reasons);
    }
}
