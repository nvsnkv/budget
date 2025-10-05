using AutoMapper;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Infrastructure.IO.Console.Converters;

internal class OwnersConverter : IValueConverter<IReadOnlyCollection<Owner>, string>
{
    public static readonly OwnersConverter Instance = new();

    public string Convert(IReadOnlyCollection<Owner> owners, ResolutionContext context) => string.Join(',', owners.Select(o => o.Name));
}
