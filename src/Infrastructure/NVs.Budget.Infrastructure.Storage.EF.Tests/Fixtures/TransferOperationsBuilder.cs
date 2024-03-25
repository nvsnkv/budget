using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Storage.Tests.Fixtures;

internal class TransferOperationsBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var fixture = new Fixture();
        fixture.SetNamedParameter("currency", Currency.Test.IsoCode);

        if (request is ParameterInfo pi)
        {
            if (pi.Name == nameof(TrackedTransfer.Source).ToLower())
            {
                var amount = fixture.Create<decimal>();
                if (amount >= 0) amount = -amount - 1;

                fixture.SetNamedParameter(nameof(amount), amount);
                return fixture.Create<Operation>();
            }

            if (pi.Name == nameof(TrackedTransfer.Sink).ToLower())
            {
                var amount = fixture.Create<decimal>();
                if (amount <= 0) amount = -amount + 1;

                fixture.SetNamedParameter(nameof(amount), amount);
                return fixture.Create<Operation>();
            }
        }

        return new NoSpecimen();
    }
}
