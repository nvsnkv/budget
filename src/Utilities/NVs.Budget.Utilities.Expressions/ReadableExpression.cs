using System.Linq.Expressions;

namespace NVs.Budget.Utilities.Expressions;

public sealed class ReadableExpression<T>
{
    private readonly string _representation;
    private readonly Expression<T> _expression;
    private readonly T _compiled;


    internal ReadableExpression(string representation, Expression<T> expression)
    {
        _representation = representation;
        _expression = expression;
        _compiled = expression.Compile();
    }

    private bool Equals(ReadableExpression<T> other)
    {
        return _representation == other._representation;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ReadableExpression<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _representation.GetHashCode();
    }

    public override string ToString()
    {
        return _representation;
    }

    public static implicit operator Expression<T>(ReadableExpression<T> value)
    {
        return value._expression;
    }

    public static implicit operator T(ReadableExpression<T> value)
    {
        return value._compiled;
    }
}
