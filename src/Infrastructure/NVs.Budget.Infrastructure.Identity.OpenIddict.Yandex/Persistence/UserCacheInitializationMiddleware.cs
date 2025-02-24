using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using NVs.Budget.Application;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Persistence;

internal class UserCacheInitializationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserCache userCache)
    {
        var authResult = await context.AuthenticateAsync();
        if (authResult.Succeeded)
        {
            await userCache.EnsureInitialized(context.RequestAborted);
        }

        await next(context);
    }
}
