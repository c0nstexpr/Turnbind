using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Turnbind.Helper;

public class Key2IndexCollection<TKey> : IReadOnlyDictionary<TKey, int> where TKey : notnull
{
    private class Node
    {
        public Node? Prev { get; private set; } = null;

        public Node? Next { get; private set; } = null;

        public int Index { get; private set; }

        public Node() { }

        public Node(Node prev)
        {
            var n = prev.Next;

            prev.Next = this;
            Prev = prev;

            if (n != null)
            {
                Next = n;
                n.Prev = this;
            }

            Index = prev.Index + 1;
        }

        public void Remove()
        {
            if (Prev != null) Prev.Next = Next;

            var next = Next;

            if (next == null) return;

            next.Prev = Prev;
            --next.Index;
            next = next.Next;

            for (; next != null; next = next.Next) --next.Index;
        }
    }

    readonly Dictionary<TKey, Node> m_dic = [];

    readonly IEnumerable<KeyValuePair<TKey, int>> m_pairsView;

    Node? m_tail = null;

    public int Count => m_dic.Count;

    public IEnumerable<TKey> Keys => m_dic.Keys;

    public IEnumerable<int> Values => m_dic.Values.Select(v => v.Index);

    public int this[TKey key] => m_dic[key].Index;

    public Key2IndexCollection() => 
        m_pairsView = m_dic.Select(kv => new KeyValuePair<TKey, int>(kv.Key, kv.Value.Index));

    public Key2IndexCollection(IEnumerable<TKey> keys) : this()
    {
        foreach (var key in keys) Add(key);
    }

    public bool Add(TKey item)
    {
        if (m_dic.ContainsKey(item)) return false;

        m_tail ??= new();
        m_dic[item] = new Node(m_tail);
        m_tail = m_dic[item];

        return true;
    }

    public bool Remove(TKey item)
    {
        if (!m_dic.TryGetValue(item, out var node)) return false;

        if (m_tail == node) m_tail = m_tail.Prev;

        node.Remove();
        m_dic.Remove(item);

        return true;
    }

    public bool ContainsKey(TKey key) => m_dic.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out int value)
    {
        if (!m_dic.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        value = node.Index;
        return true;
    }

    public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() => m_pairsView.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => m_pairsView.GetEnumerator();

    public void Clear()
    {
        m_dic.Clear();
        m_tail = null;
    }
}
