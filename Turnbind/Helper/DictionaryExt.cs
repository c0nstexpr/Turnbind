namespace Turnbind.Helper;

static class DictionaryExt
{
    public static Dictionary<U, T> ToInvDictionary<T, U>(
        this IEnumerable<KeyValuePair<T, U>> keyValuePairs
    ) where U : notnull =>
        keyValuePairs.Select(kv => new KeyValuePair<U, T>(kv.Value, kv.Key)).ToDictionary();
}
