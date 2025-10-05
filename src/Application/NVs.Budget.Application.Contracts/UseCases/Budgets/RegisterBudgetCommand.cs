using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.UseCases.Budgets;

public record RegisterBudgetCommand(UnregisteredBudget NewBudget) : IRequest<Result<TrackedBudget>>;
