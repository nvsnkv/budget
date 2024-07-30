using System.Linq.Expressions;

namespace NVs.Budget.Utilities.Expressions;

public static class ExpressionsExtensions
{
    public static Expression<Func<T, TResult>> CombineWith<T, TResult>(this Expression<Func<T, TResult>> expression, Expression<Func<T, TResult>> another) => expression.CombineWith(another, Expression.AndAlso);

    public static Expression<Func<T, TResult>> CombineWith<T, TResult>(this Expression<Func<T, TResult>> expression, Expression<Func<T, TResult>> another, Func<Expression, Expression, BinaryExpression> combineFn)
    {
        var parameter = Expression.Parameter(typeof (T));

        var leftVisitor = new ReplaceExpressionVisitor(expression.Parameters[0], parameter);
        var left = leftVisitor.Visit(expression.Body) ?? throw new NullReferenceException("Null node received while visiting expression!");

        var rightVisitor = new ReplaceExpressionVisitor(another.Parameters[0], parameter);
        var right = rightVisitor.Visit(another.Body)?? throw new NullReferenceException("Null node received while visiting another!");;

        return Expression.Lambda<Func<T, TResult>>(
            combineFn(left, right), parameter);
    }

    public static Expression<Func<TTo, bool>> ConvertTypes<TFrom, TTo>(this Expression<Func<TFrom, bool>> expression, IReadOnlyDictionary<Type, Type> mappings)
    {
        if (!mappings.Keys.Contains(typeof(TFrom))) throw new ArgumentException($"Mapping from {nameof(TFrom)} is not defined!");
        if (mappings[typeof(TFrom)] != typeof(TTo)) throw new AggregateException($"Mapping from ${typeof(TFrom)} is defined to another type!");

        var visitor = new ReplaceTypeVisitor(mappings);
        return (Expression<Func<TTo, bool>>)visitor.Visit(expression);

    }
}
