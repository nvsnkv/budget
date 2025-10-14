using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

[assembly:InternalsVisibleTo("NVs.Budget.Infrastructure.Files.CSV.Tests")]
namespace NVs.Budget.Infrastructure.Files.CSV;

public static class CsvExtensions
{
    public static IServiceCollection AddCsvFiles(this IServiceCollection services, string connectionString)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        services.AddDbContext<SettingsContext>(o => o.UseNpgsql(connectionString))
            .AddTransient<IDbMigrator, PostgreSqlDbMigrator<SettingsContext>>()
            .AddScoped<IReadingSettingsRepository, BudgetSpecificSettingsRepository>();
        return services;
    }
}
