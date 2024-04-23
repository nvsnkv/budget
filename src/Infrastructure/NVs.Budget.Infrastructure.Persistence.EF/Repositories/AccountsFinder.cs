using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class AccountsFinder : IDisposable, IAsyncDisposable
{
    private readonly Dictionary<Guid, StoredAccount?> _storedAccounts = new();
    private readonly BudgetContext _context;

    public AccountsFinder(BudgetContext context)
    {
        _context = context;
        _context.SavingChanges += ContextOnSavingChanges;
    }

    private void ContextOnSavingChanges(object? _, SavingChangesEventArgs __)
    {
        if (_context.ChangeTracker.Entries<StoredAccount>().Any(e => !e.IsKeySet || e.State == EntityState.Modified))
        {
            _storedAccounts.Clear();
        }
    }

    public async Task<StoredAccount?> FindById(Guid id, CancellationToken ct)
    {
        var account = _storedAccounts.GetValueOrDefault(id, null);
        if (account is not null)
        {
            return account;
        }

        account = await _context.Accounts.Include(a => a.Owners.Where(o => !o.Deleted))
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (account is not null)
        {
            _storedAccounts.Add(id, account);
        }

        return account;
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
