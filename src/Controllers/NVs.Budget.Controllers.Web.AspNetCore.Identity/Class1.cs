using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace NVs.Budget.Controllers.Web.AspNetCore.Identity;

public static class AspNetCoreIdentityExtensions
{
    public static IServiceCollection AddAspNetCoreIdentityUI(this IServiceCollection services)
    {
        services.AddDefaultIdentity<IdentityUser>();

        return services;
    }
}
