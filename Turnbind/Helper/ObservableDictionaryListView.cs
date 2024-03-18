using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

using ObservableCollections;

namespace Turnbind.Helper;

public sealed partial class ObservableDictionaryListView<TKey, TValue> : INotifyCollectionChanged,
    IReadOnlyDictionary<TKey, TValue>,
    IDisposable where TKey : notnull
{
    public ObservableDictionary<TKey, TValue> Dictionary { get; }

    public IEnumerable<TKey> Keys => ((IReadOnlyDictionary<TKey, TValue>)Dictionary).Keys;

    public IEnumerable<TValue> Values => ((IReadOnlyDictionary<TKey, TValue>)Dictionary).Values;

    public int Count => Dictionary.Count;

    public TValue this[TKey key] => Dictionary[key];

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

    void OnCollectionChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                OnAdd(e.NewItem);
                foreach (var item in e.NewItems) OnAdd(item);

                break;

            case NotifyCollectionChangedAction.Remove:
                OnRemove(e.OldItem.Key);
                foreach (var item in e.OldItems) OnRemove(item.Key);
                break;

            case NotifyCollectionChangedAction.Replace:
                OnReplace(e.NewItem);
                foreach (var item in e.NewItems) OnReplace(item);
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

    void OnRemove(TKey key)
    {
        var index = m_values.IndexOfKey(key);

        if (index < 0) return;

        var value = m_values.GetValueAtIndex(index);

        m_values.RemoveAt(index);

        CollectionChanged?.Invoke(
            this,
            new(
                NotifyCollectionChangedAction.Remove,
                value,
                index
            )
        );
    }

    void OnReplace(KeyValuePair<TKey, TValue> item)
    {
        var index = m_values.IndexOfKey(item.Key);

        if (index < 0) return;

        CollectionChanged?.Invoke(
            this,
            new(
                NotifyCollectionChangedAction.Replace,
                item,
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
