using AutoMapper;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.IO.Console.Converters;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Output.Operations;

namespace NVs.Budget.Infrastructure.IO.Console;

internal class CsvMappingProfile : Profile
{
    public CsvMappingProfile()
    {
        CreateMap<Operation, CsvOperation>()
            .ForMember(c => c.Amount, o => o.ConvertUsing<Money>(MoneyConverter.Instance))
            .ForMember(c => c.Budget, o => o.MapFrom(t => t.Budget.Name))
            .ForMember(c => c.Tags, o => o.ConvertUsing(TagsConverter.Instance, t => t.Tags))
            .ForMember(c => c.Attributes, o => o.ConvertUsing(AttributesConverter.Instance, t => t.Attributes));

        CreateMap<TrackedOperation, CsvTrackedOperation>()
            .ForMember(c => c.Amount, o => o.ConvertUsing<Money>(MoneyConverter.Instance))
            .ForMember(c => c.BudgetId, o => o.MapFrom(t => t.Budget.Id))
            .ForMember(c => c.Budget, o => o.MapFrom(t => t.Budget.Name))
            .ForMember(c => c.Tags, o => o.ConvertUsing(TagsConverter.Instance, t => t.Tags))
            .ForMember(c => c.Attributes, o => o.ConvertUsing(AttributesConverter.Instance, t => t.Attributes));

        CreateMap<TrackedTransfer, CsvTransfer>()
            .ForMember(c => c.SourceId, o => o.MapFrom(t => t.Source.Id))
            .ForMember(c => c.SinkId, o => o.MapFrom(t => t.Sink.Id))
            .ForMember(c => c.Fee, o => o.ConvertUsing<Money>(MoneyConverter.Instance));

        CreateMap<TrackedBudget, CsvBudget>()
            .ForMember(c => c.Owners, o => o.ConvertUsing(OwnersConverter.Instance));
    }
}
