using System.Linq.Expressions;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Services.Accounting;

internal abstract class ReckonerBase(IBudgetManager manager)
{
    protected readonly IBudgetManager Manager = manager;

    private static readonly Expression<Func<TrackedOperation, bool>> Any = _ => true;
    protected async Task<Expression<Func<TrackedOperation, bool>>> ExtendCriteria(Expression<Func<TrackedOperation, bool>>? criteria, CancellationToken ct)
    {
        criteria ??= Any;
        var budgets= await Manager.GetOwnedBudgets(ct);
        var ids = budgets.Select(a => a.Id).ToList();
        return criteria.CombineWith(t => ids.Contains(t.Budget.Id));
    }
}
