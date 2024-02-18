using System.Linq.Expressions;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Services.Accounting;

internal abstract class ReckonerBase(IAccountManager manager)
{
    protected readonly IAccountManager Manager = manager;

    private static readonly Expression<Func<TrackedTransaction, bool>> Any = _ => true;
    protected async Task<Expression<Func<TrackedTransaction, bool>>> ExtendCriteria(Expression<Func<TrackedTransaction, bool>>? criteria, CancellationToken ct)
    {
        criteria ??= Any;
        var accounts= await Manager.GetOwnedAccounts(ct);
        return criteria.CombineWith(t => accounts.Contains(t.Account));
    }
}
