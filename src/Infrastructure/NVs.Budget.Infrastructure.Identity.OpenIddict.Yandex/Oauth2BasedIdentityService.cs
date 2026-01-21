using Microsoft.AspNetCore.Authentication;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Infrastructure.Identity.Contracts;
using Microsoft.AspNetCore.Http;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

internal class Oauth2BasedIdentityService(IHttpContextAccessor accessor, IOwnersRepository ownersRepo) : IIdentityService
{
    public async Task<IUser> GetCurrentUser(CancellationToken ct)
    {
        if (accessor.HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is null!");
        }

        var result = await accessor.HttpContext.AuthenticateAsync();
        if (result.Succeeded)
        {
            var webUser = new WebUser(result.Principal);

            var owner = await ownersRepo.Get(webUser, ct);
            if (owner is not null)
            {
                return new WebUser(webUser.Id, owner);
            }
            var registrationResult = await ownersRepo.Register(new WebUser(result.Principal), ct);
            if (registrationResult.IsSuccess)
            {
                return new WebUser(webUser.Id, registrationResult.Value);
            }

            throw new InvalidOperationException("Failed to register user: " + registrationResult.Errors.Aggregate("", (s, error) => s + Environment.NewLine + error.Message));
        }

        throw new InvalidOperationException("The user is not authenticated!");
    }
}
