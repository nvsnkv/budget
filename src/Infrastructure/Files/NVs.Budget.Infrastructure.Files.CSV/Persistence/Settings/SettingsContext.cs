using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;

internal class SettingsContext(DbContextOptions<SettingsContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions  JsonOpts = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder  = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public DbSet<StoredCsvFileReadingSettings> CsvFileReadingSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("csv_settings");

        modelBuilder.UseIdentityByDefaultColumns();
        modelBuilder.HasPostgresEnum<ValidationRule.ValidationCondition>();
        modelBuilder.HasPostgresEnum<DateTimeKind>();

        modelBuilder.Entity<StoredCsvFileReadingSettings>()
            .OwnsOne(s => s.Settings, d =>
            {
                d.ToJson();
                d.Property(s => s.Attributes).HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOpts),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOpts) ?? new()
                );
                d.Property(s => s.Fields).HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOpts),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOpts) ?? new()
                );
                d.OwnsMany(s => s.Validation);
            });
    }
}
