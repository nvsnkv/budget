using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FluentResults;

namespace NVs.Budget.Utilities.Expressions;

public class ReadableExpressionsParser : ExpressionParser
{
    public static readonly ReadableExpressionsParser Default = new();

    private static readonly Regex NewLineRegex = new("\\n", RegexOptions.Compiled);
    private static readonly Regex SingleParamRegex = new(@"^\(?\s*(\w)+\s*\)?\s*=>\s*(.*)$", RegexOptions.Compiled);
    private static readonly Regex TwoParamsRegex = new(@"^\(\s*(\s*\w+)\s*,\s*(\w+)\s*\)\s*=>\s*(.*)$", RegexOptions.Compiled);

    public Result<ReadableExpression<Func<T,bool>>> ParseUnaryPredicate<T>(string input)
    {
        var parts = SingleParamRegex.Match(input);
        if (!parts.Success)
        {
            return Result.Fail(new Error("Input string does not match function format (i.e (arg) => arg == 2 )").WithMetadata("Expression", input));
        }

        var argName = parts.Groups[1].Value;
        var expressionString = parts.Groups[2].Value;

        try
        {
            var expression = Parse<Func<T, bool>>(expressionString, typeof(bool), Expression.Parameter(typeof(T), argName));
            return new ReadableExpression<Func<T, bool>>(input, expression);
        }
        catch (Exception e)
        {
            return Result.Fail(new Error($"Unable to create expression: {e.Message}").WithMetadata("Expression", input).CausedBy(e));
        }
    }

    public Result<ReadableExpression<Func<T1,T2,bool>>> ParseBinaryPredicate<T1, T2>(string input)
    {
        var parts = TwoParamsRegex.Match(NewLineRegex.Replace(input, " "));
        if (!parts.Success)
        {
            return Result.Fail(new Error("Input string does not match function format (i.e (arg1, arg2) => arg1 != arg2 )").WithMetadata("Expression", input));
        }

        var firstArg = parts.Groups[1].Value;
        var secondArg = parts.Groups[2].Value;
        var expressionString = parts.Groups[3].Value;

        try
        {
            var expression = Parse<Func<T1, T2, bool>>(expressionString, typeof(bool), Expression.Parameter(typeof(T1), firstArg),Expression.Parameter(typeof(T2), secondArg));
            return new ReadableExpression<Func<T1,T2, bool>>(input, expression);
        }
        catch (Exception e)
        {
            return Result.Fail(new Error($"Unable to create expression: {e.Message}").WithMetadata("Expression", input).CausedBy(e));
        }
    }

    public Result<ReadableExpression<Func<T,string>>> ParseUnaryConversion<T>(string input)
    {
        var parts = SingleParamRegex.Match(input);
        if (!parts.Success)
        {
            return Result.Fail(new Error("Input string does not match function format (i.e (arg) => arg.ToString() )").WithMetadata("Expression", input));
        }

        var argName = parts.Groups[1].Value;
        var expressionString = parts.Groups[2].Value;

        try
        {
            var expression = Parse<Func<T, string>>(expressionString, typeof(string), Expression.Parameter(typeof(T), argName));
            return new ReadableExpression<Func<T, string>>(input, expression);
        }
        catch (Exception e)
        {
            return Result.Fail(new Error($"Unable to create expression: {e.Message}").WithMetadata("Expression", input).CausedBy(e));
        }
    }

    public override ReadableExpressionsParser RegisterAdditionalTypes(params Type[] additionalTypes)
    {
        base.RegisterAdditionalTypes(additionalTypes);
        return this;
    }
}
