using AutoMapper;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.IO.Converters;
using NVs.Budget.Controllers.Console.IO.Models;
using NVs.Budget.Controllers.Console.IO.Output.Accounts;
using NVs.Budget.Controllers.Console.IO.Output.Operations;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.IO;

internal class CsvMappingProfile : Profile
{
    public CsvMappingProfile()
    {
        CreateMap<Operation, CsvOperation>()
            .ForMember(c => c.Amount, o => o.ConvertUsing<Money>(MoneyConverter.Instance))
            .ForMember(c => c.Account, o => o.MapFrom(t => t.Budget.Name))
            .ForMember(c => c.Tags, o => o.ConvertUsing(TagsConverter.Instance, t => t.Tags))
            .ForMember(c => c.Attributes, o => o.ConvertUsing(AttributesConverter.Instance, t => t.Attributes));

        CreateMap<TrackedOperation, CsvTrackedOperation>()
            .ForMember(c => c.Amount, o => o.ConvertUsing<Money>(MoneyConverter.Instance))
            .ForMember(c => c.AccountId, o => o.MapFrom(t => t.Budget.Id))
            .ForMember(c => c.Account, o => o.MapFrom(t => t.Budget.Name))
            .ForMember(c => c.Tags, o => o.ConvertUsing(TagsConverter.Instance, t => t.Tags))
            .ForMember(c => c.Attributes, o => o.ConvertUsing(AttributesConverter.Instance, t => t.Attributes));

        CreateMap<TrackedTransfer, CsvTransfer>()
            .ForMember(c => c.SourceId, o => o.MapFrom(t => t.Source.Id))
            .ForMember(c => c.SinkId, o => o.MapFrom(t => t.Sink.Id))
            .ForMember(c => c.Fee, o => o.ConvertUsing<Money>(MoneyConverter.Instance));

        CreateMap<TrackedBudget, CsvAccount>()
            .ForMember(c => c.Owners, o => o.ConvertUsing(OwnersConverter.Instance));
    }
}
