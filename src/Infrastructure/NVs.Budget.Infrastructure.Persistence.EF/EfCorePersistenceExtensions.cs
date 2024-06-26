﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;

namespace NVs.Budget.Infrastructure.Persistence.EF;

public static class EfCorePersistenceExtensions
{
    public static IServiceCollection AddEfCorePersistence(this IServiceCollection services, string connectionString)
    {
        services.AddAutoMapper(c => c.AddProfile(new MappingProfile()))
            .AddDbContext<BudgetContext>(o => o.UseNpgsql(connectionString))
            .AddTransient<AccountsFinder>()
            .AddSingleton<VersionGenerator>();

        services.AddTransient<IAccountsRepository, AccountsRepository>()
            .AddTransient<IExchangeRatesRepository, ExchangeRatesRepository>()
            .AddTransient<IOperationsRepository, OperationsRepository>()
            .AddTransient<IOwnersRepository, OwnersRepository>()
            .AddTransient<ITransfersRepository, TransfersRepository>()
            .AddTransient<IDbMigrator, PostgreSqlDbMigrator>()
            .AddTransient<IDbConnectionInfo, DbConnectionInfo>();

        return services;
    }
}
