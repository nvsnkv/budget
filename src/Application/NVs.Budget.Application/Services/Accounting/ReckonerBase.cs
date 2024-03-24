using System.Linq.Expressions;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Services.Accounting;

internal abstract class ReckonerBase(IAccountManager manager)
{
    protected readonly IAccountManager Manager = manager;

    private static readonly Expression<Func<TrackedOperation, bool>> Any = _ => true;
    protected async Task<Expression<Func<TrackedOperation, bool>>> ExtendCriteria(Expression<Func<TrackedOperation, bool>>? criteria, CancellationToken ct)
    {
        criteria ??= Any;
        var accounts= await Manager.GetOwnedAccounts(ct);
        var ids = accounts.Select(a => a.Id).ToList();
        return criteria.CombineWith(t => ids.Contains(t.Account.Id));
    }
}
