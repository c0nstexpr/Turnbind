using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

using ObservableCollections;

namespace Turnbind.Helper;

public sealed partial class ObservableDictionaryListView<TKey, TValue> : 
    INotifyCollectionChanged,
    IReadOnlyDictionary<TKey, TValue>,
    IDisposable where TKey : notnull
{
    public ObservableDictionary<TKey, TValue> Dictionary { get; }

    public IEnumerable<TKey> Keys => ((IReadOnlyDictionary<TKey, TValue>)Dictionary).Keys;

    public IEnumerable<TValue> Values => ((IReadOnlyDictionary<TKey, TValue>)Dictionary).Values;

    public int Count => Dictionary.Count;

    public TValue this[TKey key] => m_values[key]();

    SortedList<TKey, Func<TValue>> m_values;

    readonly IDisposable m_disposable;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    internal ObservableDictionaryListView(ObservableDictionary<TKey, TValue> dictionary)
    {
        Dictionary = dictionary;
        dictionary.CollectionChanged += OnCollectionChanged;
        m_disposable = Disposable.Create(() => dictionary.CollectionChanged -= OnCollectionChanged);
        m_values = new(GetReset());
    }

    TValue Get(TKey key) => Dictionary[key];

    Dictionary<TKey, Func<TValue>> GetReset() => Dictionary.Select(
        pair =>
            new KeyValuePair<TKey, Func<TValue>>(pair.Key, () => Get(pair.Key))
    )
        .ToDictionary();

    public int IndexOfKey(TKey key) => m_values.IndexOfKey(key);

    public TKey GetKeyAtIndex(int i) => m_values.GetKeyAtIndex(i);

    public TValue GetValueAtIndex(int i) => m_values.GetValueAtIndex(i)();

    void OnCollectionChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e)
    {
        var oldItem = e.OldItem;
        var newItem = e.NewItem;
        var oldItems = e.OldItems;
        var newItems = e.NewItems;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                OnAdd(newItem);
                foreach (var item in newItems) OnAdd(item);

                break;

            case NotifyCollectionChangedAction.Remove:
                OnRemove(oldItem);
                foreach (var item in oldItems) OnRemove(item);
                break;

            case NotifyCollectionChangedAction.Replace:
                OnReplace(oldItem, newItem);

                Debug.Assert(oldItems.Length == newItems.Length);

                for(var i = 0; i < oldItems.Length; ++i)
                    OnReplace(oldItems[i], newItems[i]);
                break;

            case NotifyCollectionChangedAction.Reset:
                m_values = new(GetReset());
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                break;
        }
    }

    void OnAdd(KeyValuePair<TKey, TValue> item)
    {
        var key = item.Key;
        var index = m_values.IndexOfKey(key);

        if (index > 0) return;

        m_values.Add(key, () => Get(key));

        CollectionChanged?.Invoke(
            this,
            new(
                NotifyCollectionChangedAction.Add,
                item,
                m_values.IndexOfKey(key)
            )
        );
    }

    void OnRemove(KeyValuePair<TKey, TValue> item)
    {
        var key = item.Key;
        var index = m_values.IndexOfKey(key);

        if (index < 0) return;

        m_values.RemoveAt(index);

        CollectionChanged?.Invoke(
            this,
            new(
                NotifyCollectionChangedAction.Remove,
                item,
                index
            )
        );
    }

    void OnReplace(KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
    {
        var key = oldItem.Key;
        var index = m_values.IndexOfKey(key);

        if (index < 0) return;

        CollectionChanged?.Invoke(
            this,
            new(
                NotifyCollectionChangedAction.Replace,
                oldItem,
                newItem,
                index
            )
        );
    }

    public KeyCollectionChanged CreateKeyView() => new(this);

    public ValueCollectionChanged CreateValueView() => new(this);

    public void Dispose() => m_disposable.Dispose();

    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => Dictionary.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();
}
