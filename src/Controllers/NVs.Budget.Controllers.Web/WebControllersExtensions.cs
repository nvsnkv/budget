using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web;

public static class WebControllersExtensions
{
    public static IServiceCollection AddWebControllers(this IServiceCollection services, ReadableExpressionsParser parser)
    {
        services.AddAutoMapper(m => m.AddProfile(new MappingProfile(parser)));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var assembly = typeof(WebControllersExtensions).Assembly;
        var part = new AssemblyPart(assembly);
        services
            .AddControllersWithViews()
            .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));

        services.AddApiVersioning();


        return services;
    }

    public static WebApplication UseWebControllers(this WebApplication app, bool useSwagger)
    {
        if (useSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        return app;
    }
}
