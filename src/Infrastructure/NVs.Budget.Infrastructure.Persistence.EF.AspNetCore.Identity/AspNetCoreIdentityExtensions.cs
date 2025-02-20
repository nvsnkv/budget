using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

namespace NVs.Budget.Infrastructure.Persistence.EF.AspNetCore.Identity;

public static class AspNetCoreIdentityExtensions
{
    public static IServiceCollection AddAspNetCoreIdentityPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(connectionString))
            .AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>();

        services.AddTransient<IDbConnectionInfo>(s => new DbConnectionInfo(s.GetRequiredService<AppIdentityDbContext>()));
        services.AddTransient<IDbMigrator>(s => new PostgreSqlDbMigrator(s.GetRequiredService<AppIdentityDbContext>()));
        services.AddScoped<IIdentityService, HttpContextBasedIdentityService>();

        return services;
    }
}
