using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;

namespace NVs.Budget.Infrastructure.Persistence.EF;

public static class EfCorePersistenceExtensions
{
    public static IServiceCollection AddEfCorePersistence(this IServiceCollection services, Action<NpgsqlDbContextOptionsBuilder>? configureOpts = null)
    {
        services.AddAutoMapper(c => c.AddProfile(new MappingProfile()));
        services.AddDbContext<BudgetContext>(o => o.UseNpgsql(configureOpts));
        services.AddSingleton<VersionGenerator>();

        services.AddTransient<IAccountsRepository, AccountsRepository>();
        services.AddTransient<IExchangeRatesRepository, ExchangeRatesRepository>();
        services.AddTransient<IOperationsRepository, OperationsRepository>();
        services.AddTransient<IOwnersRepository, OwnersRepository>();
        services.AddTransient<ITransfersRepository, TransfersRepository>();

        return services;
    }
}
