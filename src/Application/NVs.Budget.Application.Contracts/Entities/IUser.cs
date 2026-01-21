using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Application.Contracts.Entities;

public interface IUser
{
    string Id { get; }
    Owner AsOwner();
}
