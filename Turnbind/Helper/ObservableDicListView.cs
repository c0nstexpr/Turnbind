using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using ObservableCollections;

namespace Turnbind.Helper;

public sealed partial class ObservableDicListView<TKey, TValue> :
    INotifyCollectionChanged,
    IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    readonly ObservableDictionary<TKey, TValue> m_dictionary;

    SortedList<TKey, TValue> m_values;

    readonly Dictionary<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>>
        m_mappers = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add
        {
            if (value is null) return;

            void OnCollectionChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e)
            {
                var oldItem = e.OldItem;
                var newItem = e.NewItem;
                var oldItems = e.OldItems;
                var newItems = e.NewItems;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        OnAdd(value, newItem);
                        foreach (var item in newItems) OnAdd(value, item);

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        OnRemove(value, oldItem);
                        foreach (var item in oldItems) OnRemove(value, item);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        OnReplace(value, oldItem, newItem);

                        Debug.Assert(oldItems.Length == newItems.Length);

                        for (var i = 0; i < oldItems.Length; ++i)
                            OnReplace(value, oldItems[i], newItems[i]);
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        m_values = new(m_dictionary);
                        value?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                        break;
                }
            }

            m_mappers[value] = OnCollectionChanged;
            m_dictionary.CollectionChanged += OnCollectionChanged;
        }

        remove
        {
            if (value is null || !m_mappers.TryGetValue(value, out var handler))
                return;

            m_dictionary.CollectionChanged -= handler;
            m_mappers.Remove(value);
        }
    }

    public IEnumerable<TKey> Keys => m_values.Keys;

    public IEnumerable<TValue> Values => m_values.Values;

    public int Count => m_values.Count;
        
    public TValue this[TKey key] => m_values[key];

    internal ObservableDicListView(ObservableDictionary<TKey, TValue> dictionary)
    {
        m_dictionary = dictionary;
        m_values = new(m_dictionary);
    }

    public int IndexOfKey(TKey key) => m_values.IndexOfKey(key);

    public int IndexOfValue(TValue value) => m_values.IndexOfValue(value);

    public TKey GetKeyAtIndex(int i) => m_values.GetKeyAtIndex(i);

    public TValue GetValueAtIndex(int i) => m_values.GetValueAtIndex(i);

    void OnAdd(NotifyCollectionChangedEventHandler value, KeyValuePair<TKey, TValue> item)
    {
        var key = item.Key;
        var index = m_values.IndexOfKey(key);

        if (index > 0) return;

        m_values.Add(key, item.Value);

        value(
            this,
            new(
                NotifyCollectionChangedAction.Add,
                item,
                m_values.IndexOfKey(key)
            )
        );
    }

    void OnRemove(NotifyCollectionChangedEventHandler value, KeyValuePair<TKey, TValue> item)
    {
        var key = item.Key;
        var index = m_values.IndexOfKey(key);

        if (index < 0) return;

        m_values.RemoveAt(index);

        value(
            this,
            new(
                NotifyCollectionChangedAction.Remove,
                item,
                index
            )
        );
    }

    void OnReplace(NotifyCollectionChangedEventHandler value, KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
    {
        var key = oldItem.Key;
        var index = m_values.IndexOfKey(key);

        if (index < 0) return;

        m_values[key] = newItem.Value;

        value(
            this,
            new(
                NotifyCollectionChangedAction.Replace,
                newItem,
                oldItem,
                index
            )
        );
    }

    public ObservableDicKeyListView<TKey, TValue> CreateKeyView() =>
        new(this);

    public ObservableDicValueListView<TKey, TValue> CreateValueView() =>
        new(this);

    public bool ContainsKey(TKey key) => m_values.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        m_values.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => m_values.GetEnumerator();
}
