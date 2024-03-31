using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NVs.Budget.Infrastructure.Identity.AspNetCoreIdentity.Internals;

internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<Context>
{
    public Context CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql().Options;
        return new Context(options);
    }
}

internal class Context : IdentityDbContext
{
    public Context(DbContextOptions<IdentityDbContext> options):base(options) { }
}
