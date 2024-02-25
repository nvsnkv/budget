using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NVs.Budget.Infrastructure.Storage.Context;

internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<BudgetContext>
{
    public BudgetContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BudgetContext>().UseNpgsql().Options;
        return new BudgetContext(options);
    }
}
