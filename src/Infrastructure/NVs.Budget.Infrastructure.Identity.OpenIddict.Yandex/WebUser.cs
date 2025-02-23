using System.Security.Claims;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

internal class WebUser : IUser
{
    private readonly Owner _owner;
    public string Id { get; }

    public WebUser(string userId, Owner owner)
    {
        _owner = owner;
        Id = userId;
    }

    public WebUser(ClaimsPrincipal principal)
    {
        Id = principal.FindFirst(ClaimTypes.Email)?.Value ?? throw new ArgumentException("No email claim found", nameof(principal));
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        _owner = new Owner(Guid.NewGuid(), $"{name} - {Id}");
    }

    public Owner AsOwner() => _owner;
}