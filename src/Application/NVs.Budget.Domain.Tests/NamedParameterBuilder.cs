using System.Reflection;
using AutoFixture.Kernel;

namespace NVs.Budget.Domain.Tests;

internal class NamedParameterBuilder<T> : ISpecimenBuilder
{
    private readonly string _name;
    public readonly T _value;
    private readonly bool _ignoreCase;

    public NamedParameterBuilder(string name, T value, bool ignoreCase)
    {
        _name = name;
        _value = value;
        _ignoreCase = ignoreCase;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is ParameterInfo pi && pi.ParameterType == typeof(T) && _name.Equals(pi.Name, _ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
        {
            return _value;
        }

        return new NoSpecimen();
    }
}
