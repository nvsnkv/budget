using AutoFixture;
using AutoFixture.Kernel;

namespace NVs.Budget.Utilities.Testing;

public class UtcRandomDateTimeSequenceGenerator : ISpecimenBuilder
{
    private readonly ISpecimenBuilder _innerRandomDateTimeSequenceGenerator = new RandomDateTimeSequenceGenerator();

    public object Create(object request, ISpecimenContext context)
    {
        var result = _innerRandomDateTimeSequenceGenerator.Create(request, context);
        return result is NoSpecimen ? result : ((DateTime)result).ToUniversalTime();
    }
}
