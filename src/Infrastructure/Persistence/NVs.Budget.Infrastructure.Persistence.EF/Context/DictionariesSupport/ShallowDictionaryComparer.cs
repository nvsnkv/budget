using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NVs.Budget.Infrastructure.Persistence.EF.Context.DictionariesSupport;

public class ShallowDictionaryComparer()
    : ValueComparer<Dictionary<string, object>>(
        (l, r) => l != null && r != null && l.Keys.SequenceEqual(r.Keys) && l.Keys.All(k => l[k] == r[k]),
        d => d.Keys.Aggregate(0, (v, k) => HashCode.Combine(v, k.GetHashCode(), d[k].GetHashCode())),
        d => d.ToDictionary(kv => kv.Key, kv => kv.Value)
    );
