using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class BudgetsFinder : IDisposable, IAsyncDisposable
{
    private readonly Dictionary<Guid, StoredBudget?> _storedAccounts = new();
    private readonly BudgetContext _context;

    public BudgetsFinder(BudgetContext context)
    {
        _context = context;
        _context.SavingChanges += ContextOnSavingChanges;
    }

    private void ContextOnSavingChanges(object? _, SavingChangesEventArgs __)
    {
        if (_context.ChangeTracker.Entries<StoredBudget>().Any(e => !e.IsKeySet || e.State == EntityState.Modified))
        {
            _storedAccounts.Clear();
        }
    }

    public async Task<StoredBudget?> FindById(Guid id, CancellationToken ct)
    {
        var budget = _storedAccounts.GetValueOrDefault(id, null);
        if (budget is not null)
        {
            return budget;
        }

        budget = await _context.Budgets.Include(a => a.Owners.Where(o => !o.Deleted))
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (budget is not null)
        {
            _storedAccounts.Add(id, budget);
        }

        return budget;
    }

    public void Dispose()
    {
        _context.SavingChanges -= ContextOnSavingChanges;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
