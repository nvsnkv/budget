using Microsoft.EntityFrameworkCore;
using NMoneys;
using NVs.Budget.Infrastructure.Storage.Context.DictionariesSupport;
using NVs.Budget.Infrastructure.Storage.Entities;

namespace NVs.Budget.Infrastructure.Storage.Context;

internal class BudgetContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<StoredOwner> Owners { get; init; } = null!;

    public DbSet<StoredAccount> Accounts { get; init; } = null!;

    public DbSet<StoredOperation> Operations { get; init; } = null!;

    public DbSet<StoredRate> Rates { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseIdentityByDefaultColumns();
        modelBuilder.HasPostgresEnum<CurrencyIsoCode>();

        modelBuilder.Entity<StoredOwner>()
            .HasMany(o => o.Accounts)
            .WithMany(a => a.Owners);

        var rBuilder = modelBuilder.Entity<StoredRate>();
        rBuilder.HasOne(r => r.Owner).WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StoredAccount>()
            .HasMany(a => a.Operations)
            .WithOne(t => t.Account);

        var tBuilder = modelBuilder.Entity<StoredOperation>();
        tBuilder.OwnsOne(t => t.Amount);
        tBuilder.OwnsMany(t => t.Tags).WithOwner();
        tBuilder.Property(t => t.Attributes)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v.ToJsonString(),
                v => v.ToDictionary(),
                new ShallowDictionaryComparer()
            );

        base.OnModelCreating(modelBuilder);
    }
}
