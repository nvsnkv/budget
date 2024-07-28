using AutoMapper;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Controllers.Console.IO.Converters;

internal class OwnersConverter : IValueConverter<IReadOnlyCollection<Owner>, string>
{
    public static readonly OwnersConverter Instance = new();

    public string Convert(IReadOnlyCollection<Owner> owners, ResolutionContext context) => string.Join(',', owners.Select(o => o.Name));
}
