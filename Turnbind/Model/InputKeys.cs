using System.Collections;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

using LanguageExt;

namespace Turnbind.Model;

[JsonConverter(typeof(InputKeysJsonConverter))]
public readonly struct InputKeys :
    IReadOnlyList<InputKey>,
    IEquatable<InputKeys>,
    IReadOnlyDictionary<InputKey, int>
{
    readonly ImmutableDictionary<InputKey, int> m_dic;

    readonly InputKey[] m_keys;

    public InputKeys(IEnumerable<InputKey> keys)
    {
        m_dic = ImmutableDictionary.CreateRange(keys.Select((k, i) => KeyValuePair.Create(k, i)));
        m_keys = new InputKey[m_dic.Count];

        foreach (var (key, i) in m_dic) m_keys[i] = key;
    }

    public InputKeys() : this([]) { }

    public readonly int Count => m_keys.Length;

    public readonly IEnumerable<InputKey> Keys => m_dic.Keys;

    public readonly IEnumerable<int> Values => m_dic.Values;

    public readonly int this[InputKey key] => ((IReadOnlyDictionary<InputKey, int>)m_dic)[key];

    public readonly InputKey this[int index] => m_keys[index];

    public readonly bool Equals(InputKeys other) => Count == other.Count && m_keys.SequenceEqual(other.m_keys);

    public override readonly int GetHashCode()
    {
        HashCode keysCode = new();

        foreach (var key in this)
            keysCode.Add(key);

        return keysCode.ToHashCode();
    }

    public readonly IEnumerator<InputKey> GetEnumerator() => m_dic.Keys.GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly bool ContainsKey(InputKey key) => m_dic.ContainsKey(key);

    public readonly bool TryGetValue(InputKey key, out int value) => m_dic.TryGetValue(key, out value);

    readonly IEnumerator<KeyValuePair<InputKey, int>> IEnumerable<KeyValuePair<InputKey, int>>.GetEnumerator() =>
        m_dic.GetEnumerator();

    public readonly bool Empty => Count == 0;

    public override bool Equals(object? obj) => obj is InputKeys keys && Equals(keys);

    public static bool operator ==(InputKeys left, InputKeys right) => left.Equals(right);

    public static bool operator !=(InputKeys left, InputKeys right) => !(left == right);

    public override string ToString() => string.Join(" + ", m_keys.Select(k => $"{k}"));
}
