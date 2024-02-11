using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Entities.Contracts;

public interface IUser
{
    string Id { get; }
    Owner AsOwner();
}
