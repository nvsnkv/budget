using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Expressions;
using System.Reflection;
using NMoneys;

namespace NVs.Budget.Utilities.Expressions;

public class ExpressionParser
{
    private readonly ParsingConfig _config = new()
    {
        CustomTypeProvider = new TypesProvider()
    };

    public Expression<Func<T, bool>> ParsePredicate<T>(string expression, string paramName = "arg") => ParseConversion<T, bool>(expression, paramName);

    public Expression<Func<T, TResult>> ParseConversion<T, TResult>(string expression, string paramName = "arg") => Parse<Func<T, TResult>>(expression, typeof(TResult), Expression.Parameter(typeof(T), paramName));

    protected Expression<T> Parse<T>(string expression, Type resultType, params ParameterExpression[] args)
    {
        var body = new System.Linq.Dynamic.Core.Parser.ExpressionParser(args, expression, args, _config).Parse(resultType);
        return Expression.Lambda<T>(body, args);
    }

    private class TypesProvider : IDynamicLinkCustomTypeProvider
    {
        private readonly Dictionary<Type, List<MethodInfo>> _extensionMethods = new();
        private readonly HashSet<Type> _customTypes = [typeof(Money)];

        public HashSet<Type> GetCustomTypes() => _customTypes;

        public Dictionary<Type, List<MethodInfo>> GetExtensionMethods() => _extensionMethods;

        public Type? ResolveType(string typeName) => _customTypes.FirstOrDefault(t => t.Name == typeName);

        public Type? ResolveTypeBySimpleName(string simpleTypeName) => null;
    }
}
