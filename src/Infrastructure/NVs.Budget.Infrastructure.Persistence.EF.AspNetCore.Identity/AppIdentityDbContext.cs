using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Persistence.EF.AspNetCore.Identity;

internal class AppIdentityDbContext(DbContextOptions options) : IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("budget_identity");
        base.OnModelCreating(builder);
    }
}
