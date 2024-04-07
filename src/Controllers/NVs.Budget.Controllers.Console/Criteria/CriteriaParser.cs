using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Reflection;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Errors;

namespace NVs.Budget.Controllers.Console.Criteria;

internal class CriteriaParser
{
    private readonly ParsingConfig _config = new()
    {
        CustomTypeProvider = new TypesProvider()
    };

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

    public Expression<Func<T, bool>> ParsePredicate<T>(string expression, string paramName = "arg") => Parse<Func<T, bool>>(expression, typeof(bool), Expression.Parameter(typeof(T), paramName));

    private Expression<T> Parse<T>(string expression, Type resultType, params ParameterExpression[] args)
    {
        var body = new ExpressionParser(args, expression, args, _config).Parse(resultType);
        return Expression.Lambda<T>(body, args);
    }

    private class TypesProvider : IDynamicLinkCustomTypeProvider
    {
        private readonly Dictionary<Type, List<MethodInfo>> _extensionMethods = new();
        private readonly HashSet<Type> _customTypes = new HashSet<Type>() { typeof(Money) };

        public HashSet<Type> GetCustomTypes() => _customTypes;

        public Dictionary<Type, List<MethodInfo>> GetExtensionMethods() => _extensionMethods;

        public Type? ResolveType(string typeName) => _customTypes.FirstOrDefault(t => t.Name == typeName);

        public Type? ResolveTypeBySimpleName(string simpleTypeName) => null;
    }
}
