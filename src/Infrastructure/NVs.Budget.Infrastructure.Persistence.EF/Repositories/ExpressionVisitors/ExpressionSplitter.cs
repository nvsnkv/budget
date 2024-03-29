﻿using System.Linq.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.ExpressionVisitors;

internal class ExpressionSplitter
{
    private readonly DictionaryCallExcludingVisitor _excludingVisitor = new();
    private readonly DictionaryCallRewritingVisitor _rewritingVisitor = new();

    public (Expression<Func<T, bool>>, Func<T, bool>) Split<T>(Expression<Func<T, bool>> expression)
    {
        var queryable = _excludingVisitor.VisitAndConvert(expression, nameof(Split));
        expression = _rewritingVisitor.VisitAndConvert(expression, nameof(Split));

        return (queryable, expression.Compile());
    }
}
