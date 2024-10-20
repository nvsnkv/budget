using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record CalcOperationsStatisticsQuery(Criterion Criterion, Expression<Func<TrackedOperation, bool>> OperationsFilter)  : IRequest<Result<CriteriaBasedLogbook>>;
