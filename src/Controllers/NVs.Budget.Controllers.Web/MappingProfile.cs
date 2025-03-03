using AutoMapper;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Controllers;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web;

internal class MappingProfile : Profile
{
    public MappingProfile(ReadableExpressionsParser parser)
    {
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

        CreateMap<TrackedBudget, BudgetResponse>();
    }
}