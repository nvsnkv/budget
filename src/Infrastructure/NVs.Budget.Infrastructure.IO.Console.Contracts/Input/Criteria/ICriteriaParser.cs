using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

public interface ICriteriaParser
{
    Expression<Func<TrackedOperation, TrackedOperation, bool>> ParseTransferCriteria(string expression);
    Expression<Func<TrackedOperation, bool>> ParseTaggingCriteria(string expression);
    Result<Expression<Func<T, bool>>> TryParsePredicate<T>(string expression, string paramName);
    Expression<Func<T, bool>> ParsePredicate<T>(string expression, string paramName = "arg");
    Expression<Func<T, TResult>> ParseConversion<T, TResult>(string expression, string paramName = "arg");
}
