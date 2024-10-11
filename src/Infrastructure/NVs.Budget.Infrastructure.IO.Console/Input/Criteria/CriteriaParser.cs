using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

internal class CriteriaParser : ExpressionParser, ICriteriaParser
{
    public Expression<Func<TrackedOperation, TrackedOperation, bool>> ParseTransferCriteria(string expression) =>
        Parse<Func<TrackedOperation, TrackedOperation, bool>>(
            expression,
            typeof(bool),
            Expression.Parameter(typeof(TrackedOperation), "l"),
            Expression.Parameter(typeof(TrackedOperation), "r")
        );

    public Result<Expression<Func<T, bool>>> TryParsePredicate<T>(string expression, string paramName)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return (Expression<Func<T, bool>>)(_ => true);
        }
        try
        {
            return ParsePredicate<T>(expression, paramName);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionBasedError(e));
        }
    }
}
