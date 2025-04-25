namespace SecureStorage.Core.Extensions;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target,
        IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        foreach (var kvp in items)
        {
            target[kvp.Key] = kvp.Value;
        }
    }
}