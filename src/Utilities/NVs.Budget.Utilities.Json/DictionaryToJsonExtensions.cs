using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NVs.Budget.Utilities.Json;

public static class DictionaryToJsonExtensions
{
    private static readonly JsonSerializerOptions SerializeOptions = new(JsonSerializerDefaults.General);

    private static readonly JsonSerializerOptions DeserializeOptions = new(JsonSerializerDefaults.General)
    {
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode
    };


    public static string ToJsonString(this IDictionary<string, object> value) => JsonSerializer.Serialize(value, SerializeOptions);

    public static Dictionary<string, object> ToDictionary(this string value)
    {
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(value, DeserializeOptions) ?? throw new InvalidCastException("Failed to deserialize dictionary!");
        foreach (var key in result.Keys)
        {
            if (result[key] is JsonNode node)
            {
                var converted = TryConvert(node);
                if (converted is not null)
                {
                    result[key] = converted;
                }
            }
        }

        return result;
    }

    private static object? TryConvert(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        switch (node.GetValueKind())
        {
            case JsonValueKind.Object:
                var result = new Dictionary<string, object?>();
                var obj = node.AsObject();
                foreach (var (k, v) in obj.AsEnumerable())
                {
                    var value = TryConvert(v);
                    if (value != null)
                    {
                        result[k] = value;
                    }
                }

                return result;

            case JsonValueKind.Array:
                return node.AsArray().Select(TryConvert).Where(o => o is not null).ToArray();

            case JsonValueKind.String:
                return (string?)node ?? string.Empty;

            case JsonValueKind.Number:
                return (decimal)node;

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            default:
                throw new ArgumentOutOfRangeException(nameof(node), "Unsupported JsonValueKind!");
        }
    }
}
