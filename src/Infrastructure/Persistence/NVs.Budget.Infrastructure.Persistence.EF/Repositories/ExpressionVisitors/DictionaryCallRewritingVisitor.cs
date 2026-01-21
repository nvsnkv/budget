using System.Linq.Expressions;
using System.Reflection;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.ExpressionVisitors;

internal class DictionaryCallRewritingVisitor : ExpressionVisitor
{
    private static readonly MethodInfo IndexGetter = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .First(p => p.GetIndexParameters().Length > 0)
        .GetMethod!;

    private static readonly MethodInfo ContainsKeyMethod = typeof(IDictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.ContainsKey))!;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method == IndexGetter)
        {
            Expression<Func<object>> a = () => true ? new object() : null;
            return Expression.Condition(
                Expression.Call(node.Object, ContainsKeyMethod, node.Arguments),
                node,
                Expression.Constant(null));
        }
        return base.VisitMethodCall(node);
    }
}
