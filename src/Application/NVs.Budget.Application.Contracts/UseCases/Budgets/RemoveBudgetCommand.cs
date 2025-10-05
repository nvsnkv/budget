using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.UseCases.Budgets;

public record RemoveBudgetCommand(TrackedBudget Budget) : IRequest<Result>;
