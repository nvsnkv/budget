using AutoMapper;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web;

internal class MappingProfile : Profile
{
    public MappingProfile(ReadableExpressionsParser parser)
    {
        CreateMap<IReadOnlyCollection<TaggingCriterion>, IDictionary<string, IEnumerable<string>>>().ConvertUsing<Converter>();
        CreateMap<IReadOnlyList<TransferCriterion>, IDictionary<string, IEnumerable<TransferCriterionExpression>>>().ConvertUsing<Converter>();
        CreateMap<LogbookCriteria, IDictionary<string, LogbookCriteriaExpression>>().ConvertUsing<Converter>();
        CreateMap<TrackedBudget, BudgetConfiguration>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TaggingCriteria))
            .ForMember(dest => dest.Transfers, opt => opt.MapFrom(src => src.TransferCriteria))
            .ForMember(dest => dest.Logbook, opt => opt.MapFrom(src => src.LogbookCriteria));
    }
}

internal class Converter : ITypeConverter<IEnumerable<TaggingCriterion>, IDictionary<string, IEnumerable<string>>>,
    ITypeConverter<IEnumerable<TransferCriterion>, IDictionary<string, IEnumerable<TransferCriterionExpression>>>,
    ITypeConverter<LogbookCriteria, IDictionary<string, LogbookCriteriaExpression>>
{
    public IDictionary<string, IEnumerable<string>> Convert(IEnumerable<TaggingCriterion> source, IDictionary<string, IEnumerable<string>> destination, ResolutionContext context)
    {
        return source.GroupBy(x => x.Tag.ToString()).ToDictionary(x => x.Key, x => x.Select(y => y.Condition.ToString()));
    }

    public IDictionary<string, IEnumerable<TransferCriterionExpression>> Convert(IEnumerable<TransferCriterion> source, IDictionary<string, IEnumerable<TransferCriterionExpression>> _, ResolutionContext __)
    {
        return source.GroupBy(x => x.Comment).ToDictionary(
            g => g.Key,
            g => g.GroupBy(g => g.Accuracy).Select(g => new TransferCriterionExpression
            {
                Accuracy = g.Key,
                Criteria = g.Select(x => x.Criterion.ToString())
            }));
    }

    public IDictionary<string, LogbookCriteriaExpression> Convert(LogbookCriteria source, IDictionary<string, LogbookCriteriaExpression> _, ResolutionContext __)
    {
        return source.Subcriteria?.ToDictionary(s => s.Description, ConvertCriteria) ?? new();
    }

    private LogbookCriteriaExpression ConvertCriteria(LogbookCriteria criteria)
    {
        var subcriteria = criteria.Subcriteria?.ToDictionary(s => s.Description, ConvertCriteria);
        return new LogbookCriteriaExpression
        {
            Subcriteria = subcriteria,
            Type = criteria.Type,
            Tags = criteria.Tags?.Select(t => t.ToString()),
            Substitution = criteria.Substitution?.ToString(),
            Criteria = criteria.Criteria?.ToString()
        };
    }
}
