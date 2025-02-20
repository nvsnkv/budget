using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF;

public static class EfCorePersistenceExtensions
{
    public static IServiceCollection AddEfCorePersistence(this IServiceCollection services, string connectionString, ReadableExpressionsParser parser)
    {
        services.AddAutoMapper(c => c.AddProfile(new MappingProfile(parser)))
            .AddDbContext<BudgetContext>(o => o.UseNpgsql(connectionString))
            .AddTransient<BudgetsFinder>()
            .AddSingleton<VersionGenerator>();

        services.AddTransient<IBudgetsRepository, BudgetsRepository>()
            .AddTransient<IBudgetSpecificSettingsRepository, BudgetSpecificSettingsRepository>()
            .AddTransient<IExchangeRatesRepository, ExchangeRatesRepository>()
            .AddTransient<IStreamingOperationRepository, OperationsRepository>()
            .AddTransient<IOwnersRepository, OwnersRepository>()
            .AddTransient<ITransfersRepository, TransfersRepository>()
            .AddTransient<IDbMigrator>(s => new PostgreSqlDbMigrator(s.GetRequiredService<BudgetContext>()))
            .AddTransient<IDbConnectionInfo>(s => new DbConnectionInfo(s.GetRequiredService<BudgetContext>()));

        return services;
    }
}
