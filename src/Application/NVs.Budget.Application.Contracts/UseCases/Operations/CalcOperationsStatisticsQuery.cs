using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record CalcOperationsStatisticsQuery(
    Criterion Criterion,
    Expression<Func<TrackedOperation, bool>> OperationsFilter,
    Currency? OutputCurrency = null,
    bool ExcludeTransfers = true) : IRequest<Result<CriteriaBasedLogbook>>;
