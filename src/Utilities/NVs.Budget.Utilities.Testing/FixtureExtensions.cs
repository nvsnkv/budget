using AutoFixture;

namespace NVs.Budget.Utilities.Testing;

public static class FixtureExtensions
{
    public static Fixture ResetNamedParameter<T>(this Fixture fixture, string name)
    {
        var targets = fixture.Customizations.Where(c => c is NamedParameterBuilder<T> builder && builder.Name == name).ToList();
        foreach (var target in targets)
        {
            fixture.Customizations.Remove(target);
        }

        return fixture;
    }

    public static IDisposable SetNamedParameter<T>(this Fixture fixture, string name, T value)
    {
        fixture.ResetNamedParameter<T>(name);
        fixture.Customizations.Add(new NamedParameterBuilder<T>(name, value, false));
        return new Scope<T>(fixture, name);
    }

    private class Scope<T>(Fixture fixture, string name) : IDisposable
    {
        public void Dispose()
        {
            fixture.ResetNamedParameter<T>(name);
        }
    }
}
