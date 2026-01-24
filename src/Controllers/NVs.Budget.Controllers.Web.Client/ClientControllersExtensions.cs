using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NVs.Budget.Controllers.Web.Client;

public static class ClientControllersExtensions
{
    public static IServiceCollection AddClientControllers(this IServiceCollection services)
    {
        // No services need to be registered for static file serving
        // The middleware handles everything
        return services;
    }

    public static WebApplication UseClientControllers(this WebApplication app)
    {
        // Enable default files (index.html) and static file serving
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Runtime configuration endpoint for Angular app
        app.MapGet("/api/config", (IConfiguration configuration) =>
        {
            var apiUrl = configuration["ApiUrl"] ?? "https://localhost:25001";
            return Results.Ok(new { apiUrl });
        });

        // Fallback to index.html for client-side routing
        // This should be called after API routes but before other routes
        app.MapFallbackToFile("index.html");

        return app;
    }
}
