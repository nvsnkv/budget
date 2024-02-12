using System.Linq.Expressions;

namespace NVs.Budget.Utilities.Expressions;

public static class ExpressionsExtensions
{
    public static Expression<Func<T, bool>> CombineWith<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right) =>
        Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left, Expression.Lambda<Func<T, bool>>(right, left.Parameters)),
            left.Parameters);
}
