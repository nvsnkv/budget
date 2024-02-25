using System.Linq.Expressions;

namespace NVs.Budget.Utilities.Expressions;

internal class ReplaceExpressionVisitor(Expression oldValue, Expression newValue) : ExpressionVisitor
{
    public override Expression? Visit(Expression? node) => node == oldValue ? newValue : base.Visit(node);
}