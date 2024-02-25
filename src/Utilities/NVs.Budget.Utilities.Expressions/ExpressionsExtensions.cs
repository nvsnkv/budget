using System.Linq.Expressions;

namespace NVs.Budget.Utilities.Expressions;

public static class ExpressionsExtensions
{
    public static Expression<Func<T, bool>> CombineWith<T>(this Expression<Func<T, bool>> expression, Expression<Func<T, bool>> another)
    {
        var parameter = Expression.Parameter(typeof (T));

        var leftVisitor = new ReplaceExpressionVisitor(expression.Parameters[0], parameter);
        var left = leftVisitor.Visit(expression.Body) ?? throw new NullReferenceException("Null node received while visiting expression!");

        var rightVisitor = new ReplaceExpressionVisitor(another.Parameters[0], parameter);
        var right = rightVisitor.Visit(another.Body)?? throw new NullReferenceException("Null node received while visiting another!");;

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    public static Expression<Func<TTo, bool>> ConvertTypes<TFrom, TTo>(this Expression<Func<TFrom, bool>> expression, IReadOnlyDictionary<Type, Type> mappings)
    {
        if (!mappings.Keys.Contains(typeof(TFrom))) throw new ArgumentException($"Mapping from {nameof(TFrom)} is not defined!");
        if (mappings[typeof(TFrom)] != typeof(TTo)) throw new AggregateException($"Mapping from ${typeof(TFrom)} is defined to another type!");

        var visitor = new ReplaceTypeVisitor(mappings);
        return (Expression<Func<TTo, bool>>)visitor.Visit(expression);

    }
}
