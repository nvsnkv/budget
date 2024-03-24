using System.Linq.Expressions;
using NVs.Budget.Infrastructure.Storage.Entities;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal class ExpressionSplitter
{
    private readonly ExcludeDictionariesVisitor _visitor = new();

    public (Expression<Func<StoredTransaction, bool>>, Func<StoredTransaction, bool>) Split(Expression<Func<StoredTransaction, bool>> expression)
    {
        var queryable = _visitor.VisitAndConvert(expression, nameof(Split));

        return (queryable, expression.Compile());
    }
}

internal class ExcludeDictionariesVisitor : ExpressionVisitor
{
    public override Expression? Visit(Expression? node)
    {
        if (node is BinaryExpression binaryNode)
        {
            if (ShouldDetach(binaryNode))
            {
                return Expression.Constant(true);
            }
        }
        return base.Visit(node);
    }

    private bool ShouldDetach(BinaryExpression binaryNode)
        => IsDictionaryMethodCall(binaryNode.Left) || IsDictionaryMethodCall(binaryNode.Right);

    private bool IsDictionaryMethodCall(Expression node)
        => node is not MethodCallExpression mc || (mc.Method.DeclaringType?.IsAssignableTo(typeof(Dictionary<string, object>)) ?? false);
}
