using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Reflection;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;

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
