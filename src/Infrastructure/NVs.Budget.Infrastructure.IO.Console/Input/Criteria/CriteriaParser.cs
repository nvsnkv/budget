using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

internal class CriteriaParser : ExpressionParser, ICriteriaParser
{
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
