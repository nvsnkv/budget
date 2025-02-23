using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Mapping;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using OpenIddict.Client.WebIntegration;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

public static class WebIdentityExtensions
{
    private static class URIs
    {
        public static readonly string YandexRedirectUri = "callback/login/yandex";
    }

    public static IServiceCollection AddYandexAuth(this IServiceCollection services, YandexAuthConfig config, string connectionString)
    {
        services.AddScoped<IIdentityService, Oauth2BasedIdentityService>();

        services.AddDbContext<UserMappingContext>(ops => ops.UseNpgsql(connectionString));
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddHttpContextAccessor();

        services.AddDbContext<UserMappingContext>(ops =>
        {
            ops.UseNpgsql(connectionString);
            ops.UseOpenIddict();
        });
        services.AddTransient<IDbMigrator, PostgreSqlDbMigrator<UserMappingContext>>();

        services.AddOpenIddict().AddCore(opts => opts.UseEntityFrameworkCore().UseDbContext<UserMappingContext>());

        services.AddOpenIddict()
            .AddClient(opts =>
            {
                opts.AllowAuthorizationCodeFlow()
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                opts.UseAspNetCore().EnableRedirectionEndpointPassthrough();

                opts.UseWebProviders()
                    .AddYandex(yopts =>
                    {
                        yopts.SetClientId(config.ClientId);
                        yopts.SetClientSecret(config.ClientSecret);
                        yopts.SetRedirectUri(URIs.YandexRedirectUri);
                    });
            });

       return services;
    }

    public static WebApplication UseYandexAuth(this WebApplication app, string authRedirectUri)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("challenge", () => Results.Challenge(properties: null, authenticationSchemes: [OpenIddictClientWebIntegrationConstants.Providers.Yandex]));
        app.MapMethods(URIs.YandexRedirectUri, [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictClientWebIntegrationConstants.Providers.Yandex);
            return !result.Succeeded ? Results.BadRequest(result.Failure?.Message) : Results.Redirect(authRedirectUri);
        });

        app.MapGet("whoami", async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync();
            return Results.Text(result is not { Succeeded: true }
                ? "You're not logged in."
                : $"You are {result.Principal.FindFirst(ClaimTypes.Name)!.Value}.");
        });

        return app;
    }
}
