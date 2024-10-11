using Microsoft.EntityFrameworkCore;
using NMoneys;
using NVs.Budget.Infrastructure.Persistence.EF.Context.DictionariesSupport;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Utilities.Json;

namespace NVs.Budget.Infrastructure.Persistence.EF.Context;

internal class BudgetContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<StoredOwner> Owners { get; init; } = null!;

    public DbSet<StoredBudget> Budgets { get; init; } = null!;

    public DbSet<StoredOperation> Operations { get; init; } = null!;

    public DbSet<StoredRate> Rates { get; init; } = null!;

    public DbSet<StoredTransfer> Transfers { get; init; } = null!;

    public DbSet<StoredCsvFileReadingOption> CsvFileReadingOptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("budget");

        modelBuilder.UseIdentityByDefaultColumns();
        modelBuilder.HasPostgresEnum<CurrencyIsoCode>();

        var ownBuilder = modelBuilder.Entity<StoredOwner>();
        ownBuilder
            .HasMany(o => o.Accounts)
            .WithMany(a => a.Owners);

        ownBuilder.HasIndex(o => o.UserId);

        var sBuilder = modelBuilder.Entity<StoredCsvFileReadingOption>();
        sBuilder.OwnsMany<StoredFieldConfiguration>(o => o.FieldConfigurations);
        sBuilder.OwnsMany<StoredFieldConfiguration>(o => o.AttributesConfiguration);
        sBuilder.OwnsMany<StoredValidationRule>(o => o.ValidationRules);

        var rBuilder = modelBuilder.Entity<StoredRate>();
        rBuilder.HasOne(r => r.Owner).WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        var bBuilder = modelBuilder.Entity<StoredBudget>();
        bBuilder.HasMany(a => a.Operations).WithOne(t => t.Budget);
        bBuilder.HasMany<StoredCsvFileReadingOption>(b => b.CsvReadingOptions).WithOne(o => o.Budget);
        bBuilder.OwnsMany<StoredTaggingRule>(b => b.TaggingRules).WithOwner(t => t.Budget);


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
