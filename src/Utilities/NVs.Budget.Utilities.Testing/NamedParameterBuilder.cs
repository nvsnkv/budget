using System.Reflection;
using AutoFixture.Kernel;

namespace NVs.Budget.Utilities.Testing;

public sealed class NamedParameterBuilder<T>(string name, T? value, bool ignoreCase) : ISpecimenBuilder
{
    public string Name => name;

    public object? Create(object request, ISpecimenContext context)
    {
        if (request is ParameterInfo pi && pi.ParameterType == typeof(T) && name.Equals(pi.Name, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
        {
            return value;
        }

        return new NoSpecimen();
    }
}
