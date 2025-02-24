using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Mapping;

internal class UserMappingContext(DbContextOptions<UserMappingContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("user_mapping");
        modelBuilder.UseOpenIddict();

        base.OnModelCreating(modelBuilder);
    }
}
