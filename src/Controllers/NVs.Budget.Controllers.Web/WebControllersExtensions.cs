using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace NVs.Budget.Controllers.Web;

public static class WebControllersExtensions
{
    public static IServiceCollection AddWebControllers(this IServiceCollection services)
    {
        var assembly = typeof(WebControllersExtensions).Assembly;
        var part = new AssemblyPart(assembly);
        services
            .AddControllersWithViews()
            .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));

        services.AddApiVersioning();

        return services;
    }
}
