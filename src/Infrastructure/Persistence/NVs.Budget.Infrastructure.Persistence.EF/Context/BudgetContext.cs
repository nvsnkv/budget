using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NMoneys;
using NVs.Budget.Infrastructure.Persistence.EF.Context.DictionariesSupport;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Utilities.Json;

namespace NVs.Budget.Infrastructure.Persistence.EF.Context;

internal class BudgetContext(DbContextOptions<BudgetContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);

    public DbSet<StoredOwner> Owners { get; init; } = null!;

    public DbSet<StoredBudget> Budgets { get; init; } = null!;

    public DbSet<StoredOperation> Operations { get; init; } = null!;

    public DbSet<StoredRate> Rates { get; init; } = null!;

    public DbSet<StoredTransfer> Transfers { get; init; } = null!;

    [Obsolete]
    public DbSet<StoredCsvFileReadingOption> CsvFileReadingOptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("budget");

        modelBuilder.UseIdentityByDefaultColumns();
        modelBuilder.HasPostgresEnum<CurrencyIsoCode>();

        var ownBuilder = modelBuilder.Entity<StoredOwner>();
        ownBuilder
            .HasMany(o => o.Budgets)
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
        bBuilder.HasMany(b => b.Operations).WithOne(t => t.Budget);
        bBuilder.HasMany<StoredCsvFileReadingOption>(b => b.CsvReadingOptions).WithOne(o => o.Budget);
        bBuilder.OwnsMany<StoredTaggingCriterion>(b => b.TaggingCriteria).WithOwner(c => c.Budget);
        bBuilder.OwnsMany<StoredTransferCriterion>(b => b.TransferCriteria).WithOwner(c => c.Budget);
        var logbookCriteriaComparer = new ValueComparer<IList<StoredLogbookCriteria>>(
            (left, right) => JsonSerializer.Serialize(left ?? new List<StoredLogbookCriteria>(), JsonOptions) == JsonSerializer.Serialize(right ?? new List<StoredLogbookCriteria>(), JsonOptions),
            value => JsonSerializer.Serialize(value ?? new List<StoredLogbookCriteria>(), JsonOptions).GetHashCode(),
            value => JsonSerializer.Deserialize<List<StoredLogbookCriteria>>(JsonSerializer.Serialize(value ?? new List<StoredLogbookCriteria>(), JsonOptions), JsonOptions) ?? new List<StoredLogbookCriteria>()
        );
        bBuilder.Property(b => b.LogbookCriteria)
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonOptions),
                value => JsonSerializer.Deserialize<List<StoredLogbookCriteria>>(value, JsonOptions) ?? new List<StoredLogbookCriteria>())
            .Metadata.SetValueComparer(logbookCriteriaComparer);

        var tBuilder = modelBuilder.Entity<StoredTransfer>();
        tBuilder.OwnsOne(t => t.Fee);

        var oBuilder = modelBuilder.Entity<StoredOperation>();
        oBuilder.OwnsOne(t => t.Amount);
        oBuilder.OwnsMany(t => t.Tags, tags =>
        {
            tags.WithOwner().HasForeignKey("StoredOperationId");
            tags.ToTable("Operations_Tags");
        });
        oBuilder.Property(t => t.Attributes)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v.ToJsonString(),
                v => v.ToDictionary(),
                new ShallowDictionaryComparer()
            );
        oBuilder.HasOne(o => o.SourceTransfer)
            .WithOne(t => t.Source)
            .HasForeignKey<StoredTransfer>("SourceId");

        oBuilder.HasOne(o => o.SinkTransfer)
            .WithOne(t => t.Sink)
            .HasForeignKey<StoredTransfer>("SinkId");




        base.OnModelCreating(modelBuilder);
    }
}
