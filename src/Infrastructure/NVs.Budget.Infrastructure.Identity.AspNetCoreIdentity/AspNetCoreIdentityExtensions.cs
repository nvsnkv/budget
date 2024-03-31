using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NVs.Budget.Infrastructure.Identity.AspNetCoreIdentity.Internals;
using NVs.Budget.Infrastructure.Identity.Contracts;

namespace NVs.Budget.Infrastructure.Identity.AspNetCoreIdentity;

public static class AspNetCoreIdentityExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, Action<NpgsqlDbContextOptionsBuilder>? configureOpts = null)
    {
        services.AddDbContext<Context>(o => o.UseNpgsql(configureOpts));
        services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<Context>();
        services.AddTransient<IIdentityService, IdentityService>();

        return services;
    }
}
