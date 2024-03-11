using System.Collections;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

using LanguageExt;

namespace Turnbind.Model;

[method: JsonConstructor]
public class InputKeys(IEnumerable<InputKey> keys) : IReadOnlyList<InputKey>, IEquatable<InputKeys>
{
    readonly InputKey[] m_keys = keys.ToArray();

    readonly ImmutableHashSet<InputKey> m_set = ImmutableHashSet.CreateRange(keys);

    public InputKeys() : this([])
    {
    }

    public int Count => m_keys.Length;

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
}
