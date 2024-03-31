using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Identity.AspNetCoreIdentity.Internals;

internal class User(string id) : IUser
{
    private Owner _owner = Owner.Invalid;
    public string Id { get; } = id;
    public Owner AsOwner() => _owner;
    public void SetOwner(Owner owner) => _owner = owner;
}
