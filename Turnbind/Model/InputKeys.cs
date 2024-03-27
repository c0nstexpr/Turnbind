using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using LanguageExt;

namespace Turnbind.Model;

[JsonConverter(typeof(InputKeysJsonConverter))]
public class InputKeys : 
    IReadOnlyList<InputKey>,
    IEquatable<InputKeys>,
    IComparable<InputKeys>,
    IReadOnlyDictionary<InputKey, int>
{
    readonly ImmutableDictionary<InputKey, int> m_dic;

    readonly InputKey[] m_keys;

    public InputKeys(IEnumerable<InputKey> keys)
    {
        m_dic = ImmutableDictionary.CreateRange(keys.Select((k, i) => KeyValuePair.Create(k, i)));
        m_keys = new InputKey[m_dic.Count];

        foreach (var (key, i) in m_dic)
            m_keys[i] = key;
    }

    public InputKeys() : this([]) { }

    public int Count => m_keys.Length;

    public IEnumerable<InputKey> Keys => m_dic.Keys;

    public IEnumerable<int> Values => m_dic.Values;

    public int this[InputKey key] => ((IReadOnlyDictionary<InputKey, int>)m_dic)[key];

    public InputKey this[int index] => m_keys[index];

    public bool Equals(InputKeys? other) => Count == (other?.Count) && m_keys.SequenceEqual(other.m_keys);

    public override int GetHashCode()
    {
        HashCode keysCode = new();

        foreach (var key in this)
            keysCode.Add(key);

        return keysCode.ToHashCode();
    }

    public int CompareTo(InputKeys? other)
    {
        if (other is null) return 1;

        var i = 0;
        var count = Count;
        var otherCount = other.Count;

        for (; i < count; ++i)
        {
            if (i >= otherCount) return 1;

            var cmp = m_keys[i].CompareTo(other[i]);

            if (cmp != 0) return cmp;
        }

        return i < otherCount ? -1 : 0;
    }

    public IEnumerator<InputKey> GetEnumerator() => ((IEnumerable<InputKey>)m_keys).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_keys.GetEnumerator();

    public bool ContainsKey(InputKey key) => m_dic.ContainsKey(key);

    public bool TryGetValue(InputKey key, [MaybeNullWhen(false)] out int value) => m_dic.TryGetValue(key, out value);

    IEnumerator<KeyValuePair<InputKey, int>> IEnumerable<KeyValuePair<InputKey, int>>.GetEnumerator() => 
        m_dic.GetEnumerator();

    public override bool Equals(object? obj) => Equals(obj as InputKeys);
}
