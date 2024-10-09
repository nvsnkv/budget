using AutoMapper;
using NVs.Budget.Utilities.Json;

namespace NVs.Budget.Infrastructure.IO.Console.Converters;

internal class AttributesConverter : IValueConverter<IDictionary<string, object>, string>, IValueConverter<string, IReadOnlyDictionary<string, object>>
{
    public static readonly AttributesConverter Instance = new();

    public string Convert(IDictionary<string, object> sourceMember, ResolutionContext? _) => sourceMember.ToJsonString();
    public IReadOnlyDictionary<string, object> Convert(string sourceMember, ResolutionContext? _) => sourceMember.ToDictionary().AsReadOnly();
}
