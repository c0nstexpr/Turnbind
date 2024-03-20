using System.Collections;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

using LanguageExt;

namespace Turnbind.Model;

[JsonConverter(typeof(InputKeysJsonConverter))]
public class InputKeys : IReadOnlyList<InputKey>, IReadOnlySet<InputKey>, IEquatable<InputKeys>, IComparable<InputKeys>
{
    readonly ImmutableHashSet<InputKey> m_set;

    readonly InputKey[] m_keys;

    public InputKeys(IEnumerable<InputKey> keys)
    {
        m_keys = keys.ToArray();
        m_set = ImmutableHashSet.CreateRange(m_keys);
    }
    
    public InputKeys() : this([]) { }

    public int Count => m_keys.Length;

    [JsonIgnore]
    public InputKey this[int index] => m_keys[index];

    public bool Equals(InputKeys? other) => Count == (other?.Count) && this.SequenceEqual(other);

    public override int GetHashCode()
    {
        HashCode keysCode = new();

        foreach (var key in this)
            keysCode.Add(key);

        return keysCode.ToHashCode();
    }

    public IEnumerator<InputKey> GetEnumerator() => ((IEnumerable<InputKey>)m_keys).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => m_keys.GetEnumerator();

    public override bool Equals(object? obj) => Equals(obj as InputKeys);

    public bool Contains(InputKey key) => m_set.Contains(key);

    public bool IsProperSubsetOf(IEnumerable<InputKey> other) => m_set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<InputKey> other) => m_set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<InputKey> other) => m_set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<InputKey> other) => m_set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<InputKey> other) => m_set.Overlaps(other);

    public bool SetEquals(IEnumerable<InputKey> other) => m_set.SetEquals(other);

    public int CompareTo(InputKeys? other)
    {
        if (other is null) return 1;

        var i = 0;

        for (; i < m_keys.Length; ++i)
        {
            if (i >= m_keys.Length) return 1;

            var cmp = m_keys[i].CompareTo(other[i]);

            if(cmp != 0) return cmp;
        }

        return i < m_keys.Length ? - 1 : 0;
    }
}
