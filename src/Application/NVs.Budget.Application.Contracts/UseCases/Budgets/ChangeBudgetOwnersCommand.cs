using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Application.Contracts.UseCases.Budgets;

public record ChangeBudgetOwnersCommand(TrackedBudget Budget, IReadOnlyCollection<Owner> Owners) : IRequest<Result>;
