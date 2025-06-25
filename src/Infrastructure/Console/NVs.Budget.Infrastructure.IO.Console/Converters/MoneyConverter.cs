using AutoMapper;
using NMoneys;

namespace NVs.Budget.Infrastructure.IO.Console.Converters;

internal class MoneyConverter : IValueConverter<Money, string>, IValueConverter<string, Money>
{
    public static readonly MoneyConverter Instance = new();
    public string Convert(Money sourceMember, ResolutionContext? _) => sourceMember.AsQuantity();
    public Money Convert(string sourceMember, ResolutionContext? _) => Money.Parse(sourceMember);
}
