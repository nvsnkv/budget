using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NVs.Budget.Infrastructure.Persistence.EF.AspNetCore.Identity;

internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql().Options;
        return new AppIdentityDbContext(options);
    }
}
