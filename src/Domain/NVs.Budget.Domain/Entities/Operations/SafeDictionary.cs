using System.Collections;

namespace NVs.Budget.Domain.Entities.Operations;

internal class AttributesDictionary(IDictionary<string, object> source) : IDictionary<string, object>
{
    private static readonly object Fake = new();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => source.GetEnumerator();

    public void Add(KeyValuePair<string, object> item) => source.Add(item);
    public void Clear() => source.Clear();
    public bool Contains(KeyValuePair<string, object> item) => source.Contains(item);
    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => source.CopyTo(array, arrayIndex);
    public bool Remove(KeyValuePair<string, object> item) => source.Remove(item);
    public int Count => source.Count;
    public bool IsReadOnly => source.IsReadOnly;
    public void Add(string key, object value) => source.Add(key, value);
    public bool ContainsKey(string key) => source.ContainsKey(key);
    public bool Remove(string key) => source.Remove(key);
    public bool TryGetValue(string key, out object value) => source.TryGetValue(key, out value!);
    public ICollection<object> Values => source.Values;
    public ICollection<string> Keys => source.Keys;

    public object this[string key]
    {
        get => !ContainsKey(key) ? Fake : source[key];
        set => source[key] = value;
    }
}
