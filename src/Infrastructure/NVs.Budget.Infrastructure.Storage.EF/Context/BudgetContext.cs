﻿using Microsoft.EntityFrameworkCore;
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

    public DbSet<StoredTransfer> Transfers { get; init; } = null!;

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

        var oBuilder = modelBuilder.Entity<StoredOperation>();
        oBuilder.OwnsOne(t => t.Amount);
        oBuilder.OwnsMany(t => t.Tags).WithOwner();
        oBuilder.Property(t => t.Attributes)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v.ToJsonString(),
                v => v.ToDictionary(),
                new ShallowDictionaryComparer()
            );
        oBuilder.HasOne<StoredTransfer>(o => o.SourceTransfer);
        oBuilder.HasOne<StoredTransfer>(o => o.SinkTransfer);

        var tBuilder = modelBuilder.Entity<StoredTransfer>();
        tBuilder.OwnsOne(t => t.Fee);
        tBuilder.HasOne(t => t.Source);
        tBuilder.HasOne(t => t.Sink);

        base.OnModelCreating(modelBuilder);
    }
}
