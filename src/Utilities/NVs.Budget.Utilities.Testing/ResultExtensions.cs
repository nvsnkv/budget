using System.Text;
using FluentResults;

namespace NVs.Budget.Utilities.Testing;

public static class ResultExtensions
{
    public static string PrintoutReasons<T>(this Result<T> result)
    {
        var builder = new StringBuilder();
        result.Reasons.ForEach(reason => AppendReason(reason, builder));

        return builder.ToString();
    }

    private static void AppendReason(IReason reason, StringBuilder builder)
    {
        builder.AppendLine(reason.Message);
        foreach (var (key, value) in reason.Metadata)
        {
            builder.Append("  ");
            builder.Append(key);
            builder.Append(": ");
            builder.AppendLine(value.ToString());
        }
    }
}
