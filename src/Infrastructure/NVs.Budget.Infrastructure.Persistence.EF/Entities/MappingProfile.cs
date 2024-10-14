using AutoMapper;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class MappingProfile : Profile
{
    public static readonly IReadOnlyDictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>
    {
        { typeof(Money), typeof(StoredMoney) },
        { typeof(Tag), typeof(StoredTag) },
        { typeof(Owner), typeof(StoredOwner) },
        { typeof(TrackedOwner), typeof(StoredOwner) },
        { typeof(TrackedBudget), typeof(StoredBudget) },
        { typeof(TrackedOperation), typeof(StoredOperation) },
        { typeof(ExchangeRate), typeof(StoredRate) },
        { typeof(TrackedTransfer), typeof(StoredTransfer) }
    };

    public MappingProfile(ReadableExpressionsParser parser)
    {
        CreateMap<Currency, CurrencyIsoCode>().ConvertUsing(c => c.IsoCode);
        CreateMap<CurrencyIsoCode, Currency>().ConstructUsing(c => Currency.Get(c));

        CreateMap<Money, StoredMoney>().ReverseMap();
        CreateMap<Tag, StoredTag>().ReverseMap();

        CreateMap<Owner, StoredOwner>().ReverseMap();
        CreateMap<ReadableExpression<Func<TrackedOperation, bool>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<TrackedOperation, bool>>>().ConstructUsing(r => parser.ParseUnaryPredicate<TrackedOperation>(r).Value);
        CreateMap<ReadableExpression<Func<TrackedOperation, string>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<TrackedOperation, string>>>().ConstructUsing(r => parser.ParseUnaryConversion<TrackedOperation>(r).Value);

        CreateMap<ReadableExpression<Func<TrackedOperation, TrackedOperation, bool>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<TrackedOperation, TrackedOperation, bool>>>().ConstructUsing(
            r => parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(r).Value
        );

        CreateMap<TaggingCriterion, StoredTaggingCriterion>().ReverseMap();

        CreateMap<TransferCriterion, StoredTransferCriterion>().ReverseMap();
        CreateMap<TrackedOwner, StoredOwner>().ReverseMap();
        CreateMap<Domain.Entities.Accounts.Budget, StoredBudget>().ReverseMap();
        CreateMap<TrackedBudget, StoredBudget>().ReverseMap();
        CreateMap<Operation, StoredOperation>().ReverseMap();

        CreateMap<TrackedOperation, StoredOperation>()
            .ForMember(
                s => s.Timestamp,
                c => c.ConvertUsing(ToUniversalTimeConverter.Instance)
            );
        CreateMap<StoredOperation, TrackedOperation>()
            .ForMember(
                s => s.Timestamp,
                c => c.ConvertUsing(FromUniversalTimeConverter.Instance)
            );


        CreateMap<TrackedTransfer, StoredTransfer>().ReverseMap();

        CreateMap<ExchangeRate, StoredRate>().ReverseMap();
    }

    private class ToUniversalTimeConverter : IValueConverter<DateTime, DateTime>
    {
        public static readonly ToUniversalTimeConverter Instance = new();
        public DateTime Convert(DateTime sourceMember, ResolutionContext context) => sourceMember.ToUniversalTime();
    }

    private class FromUniversalTimeConverter : IValueConverter<DateTime, DateTime>
    {
        public static readonly FromUniversalTimeConverter Instance = new();
        public DateTime Convert(DateTime sourceMember, ResolutionContext context) => sourceMember.ToLocalTime();
    }
}
