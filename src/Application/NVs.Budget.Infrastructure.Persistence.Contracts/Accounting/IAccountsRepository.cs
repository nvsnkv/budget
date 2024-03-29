﻿using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IAccountsRepository
{
    Task<IReadOnlyCollection<TrackedAccount>> Get(Expression<Func<TrackedAccount, bool>> filter, CancellationToken ct);
    Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct);
    Task<Result<TrackedAccount>> Update(TrackedAccount account, CancellationToken ct);
    Task<Result> Remove(TrackedAccount account, CancellationToken ct);
}
