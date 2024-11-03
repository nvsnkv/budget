using System.Collections;
using System.Globalization;

namespace NVs.Budget.Infrastructure.IO.Console.Options;

public class CsvFileReadingOptions(
    IDictionary<string, FieldConfiguration> configurations,
    CultureInfo culture,
    DateTimeKind dateTimeKind = DateTimeKind.Local,
    IReadOnlyDictionary<string, FieldConfiguration>? attributesConfiguration = null,
    IReadOnlyDictionary<string, ValidationRule>? validationRules = null
) : IReadOnlyDictionary<string, FieldConfiguration>
{
    public bool ContainsKey(string key) => configurations.ContainsKey(key);

    public bool TryGetValue(string key, out FieldConfiguration value) => configurations.TryGetValue(key, out value!);

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public FieldConfiguration? this[string fieldName] => configurations.TryGetValue(fieldName, out FieldConfiguration? value) ? value : null;
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

    public IEnumerable<string> Keys => configurations.Keys;
    public IEnumerable<FieldConfiguration> Values => configurations.Values;
    public IReadOnlyDictionary<string, FieldConfiguration>? Attributes => attributesConfiguration;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public IReadOnlyDictionary<string, ValidationRule>? ValidationRules => validationRules;
    public IEnumerator<KeyValuePair<string, FieldConfiguration>> GetEnumerator() => configurations.GetEnumerator();
    public int Count => configurations.Count;

    public CultureInfo CultureInfo { get; } = culture;

    public DateTimeKind DateTimeKind { get; } = dateTimeKind;
}
