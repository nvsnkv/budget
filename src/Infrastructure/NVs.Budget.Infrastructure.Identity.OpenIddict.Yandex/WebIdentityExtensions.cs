using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Mapping;
using NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Persistence;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
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

                opts.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();

                opts.UseSystemNetHttp();

                opts.UseWebProviders()
                    .AddYandex(yopts =>
                    {
                        yopts.SetClientId(config.ClientId);
                        yopts.SetClientSecret(config.ClientSecret);
                        yopts.SetRedirectUri(URIs.YandexRedirectUri);
                    });
            });

        services.AddAuthorization();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
        {
            o.ForwardChallenge = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddHttpContextAccessor();


       return services;
    }

    public static WebApplication UseYandexAuth(this WebApplication app, string authRedirectUri)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<UserCacheInitializationMiddleware>();

        app.MapGet("challenge", () =>
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictClientAspNetCoreConstants.Properties.ProviderName] = OpenIddictClientWebIntegrationConstants.Providers.Yandex
            });

            return Results.Challenge(properties, authenticationSchemes: [OpenIddictClientAspNetCoreDefaults.AuthenticationScheme]);
        });
        app.MapMethods(URIs.YandexRedirectUri, [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal);
                return Results.Redirect(authRedirectUri);
            }

            return Results.BadRequest(result.Failure?.Message);
        });

        app.MapGet("whoami", async (HttpContext context, UserCache cache) =>
        {
            var result = await context.AuthenticateAsync();
            return Results.Text(result is not { Succeeded: true }
                ? "You're not logged in."
                : $"You are {result.Principal.FindFirst(ClaimTypes.Name)!.Value}. Associated owner id: {cache.CachedUser.AsOwner().Id}");
        });

        return app;
    }
}
