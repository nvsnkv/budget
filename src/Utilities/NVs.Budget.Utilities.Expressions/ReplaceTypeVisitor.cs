using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace NVs.Budget.Utilities.Expressions;

internal class ReplaceTypeVisitor : ExpressionVisitor
{
    private readonly IReadOnlyDictionary<Type, Type> _mapping;
    private readonly Stack<ReadOnlyCollection<ParameterExpression>> _parameters = new();

    public ReplaceTypeVisitor(IReadOnlyDictionary<Type, Type> mapping)
    {
        _mapping = mapping;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        _parameters.Push(VisitAndConvert(node.Parameters, nameof(VisitLambda)));
        var result = Expression.Lambda(Visit(node.Body), _parameters.Peek());
        _parameters.Pop();
        return result;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {


        if (node.Method.IsGenericMethod && RequiresReplacement(node.Method.GetGenericArguments()))
        {
            var replacedTypeArgs = GetReplacements(node.Method.GetGenericArguments()).ToArray();
            var definition = node.Method.GetGenericMethodDefinition();
            var method = definition.MakeGenericMethod(replacedTypeArgs);
            return Expression.Call(node.Object, method, node.Arguments.Select(Visit)!);
        }

        return base.VisitMethodCall(node);
    }

    private IEnumerable<Type> GetReplacements(Type[] sourceTypes)
    {
        foreach (var sourceType in sourceTypes)
        {
            if (_mapping.TryGetValue(sourceType, out var dest))
            {
                yield return dest;
            }
            else if (sourceType.IsGenericType)
            {
                var subtypes = GetReplacements(sourceType.GetGenericArguments());
                var definition = sourceType.GetGenericTypeDefinition();
                yield return definition.MakeGenericType(subtypes.ToArray());
            }
            else
            {
                yield return sourceType;
            }
        }
    }

    private bool RequiresReplacement(IEnumerable<Type> types)
    {
        var queue = new Queue<Type>(types);
        var seen = new HashSet<Type>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!seen.Add(current))
            {
                continue;
            }

            if (_mapping.ContainsKey(current))
            {
                return true;
            }

            if (current.IsGenericType)
            {
                foreach (var arg in current.GetGenericArguments())
                {
                    queue.Enqueue(arg);
                }
            }
        }

        return false;
    }


    protected override Expression VisitParameter(ParameterExpression node)
    {
        var stored = _parameters.TryPeek(out var prms)
               ? prms.FirstOrDefault(p => p.Name == node.Name)
               : null;

        return stored ?? TryReplaceTypes(node) ?? base.VisitParameter(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var visited = VisitAndConvert(node.Expression, nameof(VisitMember));

        var memberInfo = node.Member;
        foreach (var (source, dest)in _mapping)
        {
            if (visited?.Type == dest)
            {
                memberInfo = dest.GetProperty(node.Member.Name);
                if (memberInfo is null)
                {
                    throw new InvalidOperationException($"Cannot find {node.Member.Name} in {dest.Name}!");
                }
            }
        }

        return Expression.MakeMemberAccess(visited, memberInfo);
    }

    private ParameterExpression? TryReplaceTypes(ParameterExpression node)
    {
        foreach (var (source, dest) in _mapping)
        {
            if (node.Type == source)
            {
                return Expression.Parameter(dest, node.Name);
            }
        }

        return null;
    }
}
