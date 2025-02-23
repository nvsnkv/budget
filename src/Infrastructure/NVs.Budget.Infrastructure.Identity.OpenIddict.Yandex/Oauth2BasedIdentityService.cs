using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Infrastructure.Identity.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Mapping;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

internal class Oauth2BasedIdentityService(HttpContext httpContext, UserMappingContext mappingContext, IOwnersRepository ownersRepo) : IIdentityService
{
    public async Task<IUser> GetCurrentUser(CancellationToken ct)
    {
        var result = await httpContext.AuthenticateAsync();
        if (result.Succeeded)
        {
            var userId = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException("No email found in the claims!");
            }

            var mapping = await mappingContext.Mappings.FirstOrDefaultAsync(m => m.UserId == userId, ct);
            if (mapping != null)
            {

                var owners = await ownersRepo.Get(o => o.Id == mapping.OwnerId, ct);
                var owner = owners.FirstOrDefault();
                if (owner != null)
                {
                    return new WebUser(userId, owner);
                }

                throw new InvalidOperationException("Mapping exists, but owner was not found!");
            }

            var registrationResult = await ownersRepo.Register(new WebUser(result.Principal), ct);
            if (registrationResult.IsSuccess)
            {
                return new WebUser(userId, registrationResult.Value);
            }

            throw new InvalidOperationException("Failed to register user: " + registrationResult.Errors.Aggregate("", (s, error) => s + Environment.NewLine + error.Message));

        }

        throw new InvalidOperationException("The user is not authenticated!");
    }
}
