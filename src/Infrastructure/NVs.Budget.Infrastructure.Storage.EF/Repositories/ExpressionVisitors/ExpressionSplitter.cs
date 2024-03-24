using System.Linq.Expressions;
using NVs.Budget.Infrastructure.Storage.Entities;

namespace NVs.Budget.Infrastructure.Storage.Repositories.ExpressionVisitors;

internal class ExpressionSplitter
{
    private readonly DictionaryCallExcludingVisitor _excludingVisitor = new();
    private readonly DictionaryCallRewritingVisitor _rewritingVisitor = new();

    public (Expression<Func<StoredOperation, bool>>, Func<StoredOperation, bool>) Split(Expression<Func<StoredOperation, bool>> expression)
    {
        var queryable = _excludingVisitor.VisitAndConvert(expression, nameof(Split));
        expression = _rewritingVisitor.VisitAndConvert(expression, nameof(Split));

        return (queryable, expression.Compile());
    }
}
