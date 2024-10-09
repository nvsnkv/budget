using AutoMapper;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Infrastructure.IO.Console.Converters;

internal class TagsConverter : IValueConverter<IReadOnlyCollection<Tag>, string>, IValueConverter<string, IReadOnlyCollection<Tag>>
{
    public static readonly TagsConverter Instance = new();
    public string Convert(IReadOnlyCollection<Tag> sourceMember, ResolutionContext? _) => string.Join(", ", sourceMember.Select(s => s.Value));
    public IReadOnlyCollection<Tag> Convert(string sourceMember, ResolutionContext? _) => sourceMember.Split(',').Select(s => new Tag(s)).ToArray();
}
