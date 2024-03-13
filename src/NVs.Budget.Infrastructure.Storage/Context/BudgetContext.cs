﻿using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Storage.Entities;

namespace NVs.Budget.Infrastructure.Storage.Context;

internal class BudgetContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<StoredOwner> Owners { get; init; } = null!;

    public DbSet<StoredAccount> Accounts { get; init; } = null!;

    public DbSet<StoredTransaction> Transactions { get; init; } = null!;

    public DbSet<StoredRate> Rates { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseIdentityByDefaultColumns();

        modelBuilder.Entity<StoredOwner>()
            .HasMany(o => o.Accounts)
            .WithMany(a => a.Owners);

        var rBuilder = modelBuilder.Entity<StoredRate>();
        rBuilder.HasNoKey();
        rBuilder.HasOne(r => r.Owner).WithMany();

        modelBuilder.Entity<StoredAccount>()
            .HasMany(a => a.Transactions)
            .WithOne(t => t.Account);

        var tBuilder = modelBuilder.Entity<StoredTransaction>();
        tBuilder.OwnsOne(t => t.Amount);
        tBuilder.OwnsMany(t => t.Tags).WithOwner();
        tBuilder.OwnsOne(t => t.Attributes, d =>
        {
            d.ToJson();
        });

        base.OnModelCreating(modelBuilder);
    }
}
