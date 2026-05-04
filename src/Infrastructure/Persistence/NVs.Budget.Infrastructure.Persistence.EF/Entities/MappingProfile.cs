using AutoMapper;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
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
        { typeof(TrackedBudgetPlan), typeof(StoredBudgetPlan) },
        { typeof(TrackedOperation), typeof(StoredOperation) },
        { typeof(ExchangeRate), typeof(StoredRate) },
        { typeof(TrackedTransfer), typeof(StoredTransfer) }
    };

    public MappingProfile(ReadableExpressionsParser parser)
    {
        AllowNullCollections = true;

        CreateMap<Currency, CurrencyIsoCode>().ConvertUsing(c => c.IsoCode);
        CreateMap<CurrencyIsoCode, Currency>().ConstructUsing(c => Currency.Get(c));
        CreateMap<Money, StoredMoney>()
            .ForCtorParam(nameof(StoredMoney.Amount), opt => opt.MapFrom(s => s.Amount))
            .ForCtorParam(nameof(StoredMoney.CurrencyCode), opt => opt.MapFrom(s => s.CurrencyCode));
        CreateMap<StoredMoney, Money>()
            .ConstructUsing(s => new Money(s.Amount, Currency.Get(s.CurrencyCode)));

        CreateMap<Tag, StoredTag>().ReverseMap();
        CreateMap<Owner, StoredOwner>().ReverseMap();
        CreateMap<Domain.Entities.Budgets.Budget, StoredBudget>().ReverseMap();
        CreateMap<Operation, StoredOperation>().ReverseMap();

        CreateMap<ReadableExpression<Func<Operation, string>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<Operation, string>>>().ConstructUsing(
            r => parser.ParseUnaryConversion<Operation>(r).Value
        );
        CreateMap<ReadableExpression<Func<Operation, bool>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<Operation, bool>>>().ConstructUsing(
            r => parser.ParseUnaryPredicate<Operation>(r).Value
        );
        CreateMap<ReadableExpression<Func<TrackedOperation, bool>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<TrackedOperation, bool>>>().ConstructUsing(r => parser.ParseUnaryPredicate<TrackedOperation>(r).Value);
        CreateMap<ReadableExpression<Func<TrackedOperation, string>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<TrackedOperation, string>>>().ConstructUsing(r => parser.ParseUnaryConversion<TrackedOperation>(r).Value);

        CreateMap<ReadableExpression<Func<TrackedOperation, TrackedOperation, bool>>, string>().ConstructUsing(r => r.ToString());
        CreateMap<string, ReadableExpression<Func<TrackedOperation, TrackedOperation, bool>>>().ConstructUsing(r => parser.ParseBinaryPredicate<TrackedOperation,TrackedOperation>(r).Value);

        CreateMap<TaggingCriterion, StoredTaggingCriterion>().ReverseMap();
        CreateMap<TransferCriterion, StoredTransferCriterion>().ReverseMap();
        CreateMap<LogbookCriteria, StoredLogbookCriteria>().ReverseMap();
        CreateMap<PlanExpectation, StoredPlanExpectation>()
            .ForMember(d => d.ExpectedAmount, opt => opt.MapFrom(s => s.ExpectedAmount.Amount))
            .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.ExpectedAmount.CurrencyCode))
            .ForMember(d => d.From, opt => opt.MapFrom(s => ToUtc(s.From)))
            .ForMember(d => d.Till, opt => opt.MapFrom(s => ToUtc(s.Till)))
            .ForMember(d => d.SubcriterionPath, opt => opt.MapFrom(s => s.SubcriterionPath))
            .ForMember(d => d.SubcriterionName, opt => opt.MapFrom(s => s.SubcriterionPath));
        CreateMap<StoredPlanExpectation, PlanExpectation>()
            .ConstructUsing(s => new PlanExpectation(
                s.Id,
                new Money(s.ExpectedAmount, Currency.Get(s.CurrencyCode)),
                ToUtc(s.From),
                ToUtc(s.Till),
                s.ResolvedCriterionPath,
                s.Note));

        CreateMap<TrackedOwner, StoredOwner>().ReverseMap();
        CreateMap<TrackedBudget, StoredBudget>().ReverseMap();
        CreateMap<TrackedBudgetPlan, StoredBudgetPlan>()
            .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.IsoCode))
            .ForMember(d => d.ExpectedAmount, opt => opt.MapFrom(s => s.ExpectedAmount.Amount))
            .ForMember(d => d.From, opt => opt.MapFrom(s => ToUtc(s.From)))
            .ForMember(d => d.Till, opt => opt.MapFrom(s => ToUtc(s.Till)))
            .ForMember(d => d.LogbookCriteria, opt => opt.MapFrom(s => s.LogbookCriteria ?? new LogbookCriteria(s.Criterion.Description, null, null, null, null, null, s.Criterion is UniversalCriterion, null)))
            .ForMember(d => d.Budget, opt => opt.Ignore());
        CreateMap<StoredBudgetPlan, TrackedBudgetPlan>()
            .ConvertUsing((s, _, context) =>
            {
                var logbookCriteria = context.Mapper.Map<LogbookCriteria>(s.LogbookCriteria);
                return new TrackedBudgetPlan(
                    s.Id,
                    s.BudgetId,
                    s.Name,
                    ToUtc(s.From),
                    ToUtc(s.Till),
                    s.CronExpression,
                    logbookCriteria.GetCriterion(),
                    Currency.Get(s.CurrencyCode),
                    context.Mapper.Map<List<PlanExpectation>>(s.Expectations),
                    new Money(s.ExpectedAmount, Currency.Get(s.CurrencyCode)),
                    s.Note,
                    logbookCriteria)
                {
                    Version = s.Version
                };
            });
        CreateMap<TrackedOperation, StoredOperation>()
            .ForCtorParam(
                nameof(StoredOperation.Timestamp).ToLower(),
                opt => opt.MapFrom(t => t.Timestamp.ToUniversalTime())
            );
        CreateMap<StoredOperation, TrackedOperation>()
            .ForCtorParam(
                nameof(TrackedOperation.Timestamp).ToLower(),
                opt => opt.MapFrom(t => t.Timestamp.ToLocalTime())
            );
        CreateMap<TrackedTransfer, StoredTransfer>()
            .ForMember(s => s.StartedAt, opt => opt.MapFrom(t => t.StartedAt.ToUniversalTime()))
            .ForMember(s => s.CompletedAt, opt => opt.MapFrom(t => t.CompletedAt.ToUniversalTime()));

        CreateMap<StoredTransfer, TrackedTransfer>()
            .ForMember(t => t.StartedAt, opt => opt.MapFrom(s => s.StartedAt.ToLocalTime()))
            .ForMember(t => t.CompletedAt, opt => opt.MapFrom(s => s.CompletedAt.ToLocalTime()));
        
        CreateMap<ExchangeRate, StoredRate>().ReverseMap();
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
