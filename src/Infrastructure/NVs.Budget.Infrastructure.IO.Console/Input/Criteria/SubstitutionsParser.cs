using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

internal class SubstitutionsParser(CriteriaParser parser)
{
    private static readonly Regex SubstitutionPattern = new(@"\$\{(.+?)\}", RegexOptions.Compiled);

    public Func<TContext, string> GetSubstitutions<TContext>(string value, string paramName)
    {
        var matches = SubstitutionPattern.Matches(value);
        if (matches.Count == 0)
        {
            return _ => value;
        }

        var chunks = GenerateChunks<TContext>(value, matches, paramName);

        var result = chunks.Aggregate((acc, c) => acc.CombineWith(c, ConcatFn));
        return result.Compile();
    }

    private BinaryExpression ConcatFn(Expression l, Expression r)
        => Expression.Add(l, r, typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) }));

    private IEnumerable<Expression<Func<TContext, string>>> GenerateChunks<TContext>(string value, MatchCollection matches, string paramName)
    {
        var idx = 0;
        foreach (Match match in matches)
        {
            if (idx < match.Index)
            {
                var chunkStart = idx;
                yield return _ => value.Substring(chunkStart, match.Index - chunkStart);
                idx = match.Index;
            }

            yield return parser.ParseConversion<TContext, string>(match.Groups[1].Value, paramName);

            idx += match.Length;
        }

        if (idx < value.Length)
        {
            yield return _ => value.Substring(idx);
        }
    }
}
