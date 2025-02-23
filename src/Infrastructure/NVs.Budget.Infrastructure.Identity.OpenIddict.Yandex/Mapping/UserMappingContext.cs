using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Mapping;

internal class UserMappingContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<UserToOwnerMapping> Mappings { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserToOwnerMapping>()
            .HasKey(mapping => new { mapping.UserId, mapping.OwnerId });

        modelBuilder.HasDefaultSchema("user_mapping");

        base.OnModelCreating(modelBuilder);
    }
}
