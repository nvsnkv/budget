using System.Linq.Expressions;

namespace NVs.Budget.Infrastructure.Storage.Repositories.ExpressionVisitors;

internal class DictionaryCallExcludingVisitor : ExpressionVisitor
{
    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.Type == typeof(bool))
        {
            var replaceLeft = HasDictionaryMethodCall(node.Left);
            var replaceRight = HasDictionaryMethodCall(node.Right);

            if (replaceLeft && replaceRight)
            {
                return Expression.Constant(true);
            }

            if (replaceLeft)
            {
                return node.Right.Type == typeof(bool) ? node.Right : Expression.Constant(true);
            }

            if (replaceRight)
            {
                return node.Left.Type == typeof(bool) ? node.Left : Expression.Constant(true);
            }
        }

        return base.VisitBinary(node);
    }

    private bool HasDictionaryMethodCall(Expression? node)
    {
        if (node is UnaryExpression u)
        {
            return HasDictionaryMethodCall(u.Operand);
        }

        if (node is not MethodCallExpression mc) return false;

        if (mc.Method.DeclaringType?.IsAssignableTo(typeof(IDictionary<string, object>)) ?? false)
        {
            return true;
        }

        return HasDictionaryMethodCall(mc.Object);
    }
}
