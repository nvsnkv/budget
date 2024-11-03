using System.Linq.Expressions;
using FluentResults;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

public interface ICriteriaParser
{
    Result<Expression<Func<T, bool>>> TryParsePredicate<T>(string expression, string paramName);
}
