using AutoMapper;
using NMoneys;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class MappingProfile : Profile
{
    public static readonly IReadOnlyDictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>()
    {
        { typeof(Money), typeof(StoredMoney) },
        { typeof(Tag), typeof(StoredTag) },
        { typeof(Owner), typeof(StoredOwner) },
        { typeof(TrackedAccount), typeof(StoredAccount) },
        { typeof(TrackedOperation), typeof(StoredOperation) },
        { typeof(ExchangeRate), typeof(StoredRate) },
        { typeof(TrackedTransfer), typeof(StoredTransfer) }
    };

    public MappingProfile()
    {
        CreateMap<Currency, CurrencyIsoCode>().ConvertUsing(c => c.IsoCode);
        CreateMap<CurrencyIsoCode, Currency>().ConstructUsing(c => Currency.Get(c));

        CreateMap<Money, StoredMoney>().ReverseMap();
        CreateMap<Tag, StoredTag>().ReverseMap();

        CreateMap<Owner, StoredOwner>().ReverseMap();
        CreateMap<Account, StoredAccount>().ReverseMap();
        CreateMap<TrackedAccount, StoredAccount>().ReverseMap();
        CreateMap<Operation, StoredOperation>().ReverseMap();
        CreateMap<TrackedOperation, StoredOperation>().ReverseMap();
        CreateMap<TrackedTransfer, StoredTransfer>().ReverseMap();

        CreateMap<ExchangeRate, StoredRate>().ReverseMap();
    }
}
