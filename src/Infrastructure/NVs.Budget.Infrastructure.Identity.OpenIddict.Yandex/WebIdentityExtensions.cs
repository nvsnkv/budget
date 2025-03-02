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
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.WebIntegration;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

public static class WebIdentityExtensions
{
    // ReSharper disable once InconsistentNaming
    private static class URIs
    {
        public static readonly string YandexRedirectUri = "/auth/callback/login/yandex";
        public static readonly string ChallengeUrl = "/auth/challenge";
        public static readonly string WhoamiUrl = "/auth/whoami";
        public static readonly string LogoutUri = "/auth/logout";
        public static readonly string LoginUrl = "/auth/login";
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
            o.Cookie.HttpOnly = false;
            o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            o.ForwardChallenge = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddHttpContextAccessor();


       return services;
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static WebApplication UseYandexAuth(this WebApplication app, string authRedirectUri)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<UserCacheInitializationMiddleware>();

        app.MapGet(URIs.ChallengeUrl, () =>
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

        app.MapGet(URIs.WhoamiUrl, async (HttpContext context, UserCache cache) =>
        {
            var result = await context.AuthenticateAsync();
            return result.Succeeded
                ? Results.Ok(new WhoamiResponse(true, cache.CachedUser))
                : Results.Ok(new WhoamiResponse(false, null));
        });

        app.MapGet(URIs.LogoutUri, async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect(authRedirectUri);
        });

        app.MapGet(URIs.LoginUrl,  (HttpContext _) => Results.Redirect(URIs.ChallengeUrl));

        return app;
    }
}
