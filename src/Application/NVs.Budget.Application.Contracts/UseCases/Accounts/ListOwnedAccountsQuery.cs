using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Accounts;

public class ListOwnedAccountsQuery : IRequest<IReadOnlyCollection<TrackedAccount>>;
