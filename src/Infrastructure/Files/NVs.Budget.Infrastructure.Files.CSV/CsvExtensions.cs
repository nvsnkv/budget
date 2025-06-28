using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;

[assembly:InternalsVisibleTo("NVs.Budget.Infrastructure.Files.CSV.Tests")]
namespace NVs.Budget.Infrastructure.Files.CSV;

public static class CsvExtensions
{
    public static IServiceCollection AddCsvFiles(this IServiceCollection services)
    {
        services.AddScoped<IReadingSettingsRepository, BudgetSpecificSettingsRepository>();
        return services;
    }
}
