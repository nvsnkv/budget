using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class GetAccountStatisticsQueryHandler(IAccountManager manager, IReckoner reckoner) :IRequestHandler<CalcAccountStatisticsQuery, Result<CriteriaBasedLogbook>>
{
    public async Task<Result<CriteriaBasedLogbook>> Handle(CalcAccountStatisticsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await manager.GetOwnedAccounts(cancellationToken);
        var filtered = accounts.Where(request.AccountsFilter.Compile()).ToArray();

        var ids = filtered.Select(a => a.Id).ToArray();

        var operationsFilter = request.OperationsFilter.CombineWith(o => ids.Contains(o.Account.Id));
        var operations = reckoner.GetTransactions(new OperationQuery(operationsFilter), cancellationToken);

        var logbook = new CriteriaBasedLogbook(new AccountLogbookCriterion(filtered));

        var result = Result.Ok(logbook);

        await foreach (var operation in operations)
        {
            var r = logbook.Register(operation);
            result.WithReasons(r.Reasons);
        }

        return result;
    }
}
