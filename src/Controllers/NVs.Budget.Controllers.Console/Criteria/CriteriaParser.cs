using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Controllers.Console.Criteria;

internal class CriteriaParser
{
    private readonly ParsingConfig _config = new();
    public Expression<Func<TrackedOperation, TrackedOperation, bool>> ParseTransferCriteria(string expression) =>
        Parse<Func<TrackedOperation, TrackedOperation, bool>>(
            expression,
            typeof(bool),
            Expression.Parameter(typeof(TrackedOperation), "l"),
            Expression.Parameter(typeof(TrackedOperation), "r")
        );

    public Expression<Func<TrackedOperation, bool>> ParseTaggingCriteria(string expression) =>
        Parse<Func<TrackedOperation, bool>>(
            expression,
            typeof(bool),
            Expression.Parameter(typeof(TrackedOperation), "o")
        );

    private Expression<T> Parse<T>(string expression, Type resultType, params ParameterExpression[] args)
    {
        var body = new ExpressionParser(args, expression, args, _config).Parse(resultType);
        return Expression.Lambda<T>(body, args);
    }
}
