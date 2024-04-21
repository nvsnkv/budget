using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.Identity.Contracts;

namespace NVs.Budget.Infrastructure.Identity.Console;

public static class ConsoleIdentityExtensions
{
    public static IServiceCollection AddConsoleIdentity(this IServiceCollection services)
    {
        services.AddTransient<IIdentityService, EnvironmentBasedIdentityService>();
        return services;
    }
}