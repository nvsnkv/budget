using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Application.Contracts.UseCases.Accounts;

public record CalcAccountStatisticsQuery(Expression<Func<TrackedBudget, bool>> AccountsFilter, Expression<Func<TrackedOperation, bool>> OperationsFilter) : IRequest<Result<CriteriaBasedLogbook>>;
