using AutoMapper;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.IO.Output.Operations;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Controllers.Console.IO;

internal class CsvMappingProfile : Profile
{
    public CsvMappingProfile()
    {
        CreateMap<Operation, CsvOperation>()
            .ForMember(c => c.Amount, o => o.ConvertUsing(MoneyConverter.Instance))
            .ForMember(c => c.Account, o => o.MapFrom(t => t.Account.Name))
            .ForMember(c => c.Bank, o => o.MapFrom(t => t.Account.Bank))
            .ForMember(c => c.Tags, o => o.ConvertUsing(TagsConverter.Instance, t => t.Tags))
            .ForMember(c => c.Attributes, o => o.ConvertUsing(AttributesConverter.Instance, t => t.Attributes));

        CreateMap<TrackedOperation, CsvTrackedOperation>()
            .ForMember(c => c.Amount, o => o.ConvertUsing(MoneyConverter.Instance))
            .ForMember(c => c.AccountId, o => o.MapFrom(t => t.Account.Id))
            .ForMember(c => c.Account, o => o.MapFrom(t => t.Account.Name))
            .ForMember(c => c.Bank, o => o.MapFrom(t => t.Account.Bank))
            .ForMember(c => c.Tags, o => o.ConvertUsing(TagsConverter.Instance, t => t.Tags))
            .ForMember(c => c.Attributes, o => o.ConvertUsing(AttributesConverter.Instance, t => t.Attributes));

        CreateMap<TrackedTransfer, CsvTransfer>()
            .ForMember(c => c.SourceId, o => o.MapFrom(t => t.Source.Id))
            .ForMember(c => c.SinkId, o => o.MapFrom(t => t.Sink.Id))
            .ForMember(c => c.Fee, o => o.ConvertUsing(MoneyConverter.Instance));

    }

    private class MoneyConverter : IValueConverter<Money, string>
    {
        public static readonly MoneyConverter Instance = new();
        public string Convert(Money sourceMember, ResolutionContext context) => sourceMember.ToString();
    }
    private class TagsConverter : IValueConverter<IReadOnlyCollection<Tag>, string>
    {
        public static readonly TagsConverter Instance = new();
        public string Convert(IReadOnlyCollection<Tag> sourceMember, ResolutionContext context) => string.Join(", ", sourceMember.Select(s => s.Value));
    }

    private class AttributesConverter : IValueConverter<IDictionary<string, object>, string>
    {
        public static readonly AttributesConverter Instance = new();
        public string Convert(IDictionary<string, object> sourceMember, ResolutionContext context) => string.Join(", ", sourceMember.Select(s => $"({s.Key}: {s.Value})"));
    }
}
