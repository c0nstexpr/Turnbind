using System.Collections;
using System.Collections.Specialized;

namespace Turnbind.Helper;

public sealed class ObservableDicKeyListView<TKey, TValue> : 
    INotifyCollectionChanged,
    IReadOnlyList<TKey>
    where TKey : notnull
{
    readonly ObservableDicListView<TKey, TValue> m_view;

    public int Count => m_view.Count;

    public bool IsReadOnly => true;

    public TKey this[int i] => m_view.GetKeyAtIndex(i);

    readonly Dictionary<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventHandler>
        m_mappers = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add
        {
            if (value is null || m_mappers.ContainsKey(value)) return;

            void Handler(object? _, NotifyCollectionChangedEventArgs e)
            {
                var newItems = Select(e.NewItems);
                var oldItems = Select(e.OldItems);

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        value(
                            this,
                            new(
                                NotifyCollectionChangedAction.Add,
                                newItems,
                                e.NewStartingIndex
                            )
                        );
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        value(
                            this,
                            new(
                                NotifyCollectionChangedAction.Remove,
                                oldItems,
                                e.OldStartingIndex
                            )
                        );
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        value(this, new(NotifyCollectionChangedAction.Reset));
                        break;
                }
            }

            m_mappers[value] = Handler;

            m_view.CollectionChanged += Handler;
        }

        remove
        {
            if (value is null || !m_mappers.TryGetValue(value, out var handler))
                return;

            m_view.CollectionChanged -= handler;
            m_mappers.Remove(value);
        }
    }

    internal ObservableDicKeyListView(ObservableDicListView<TKey, TValue> view) => m_view = view;
    static TKey[] Select(IList? list) => list?.Cast<KeyValuePair<TKey, TValue>>()
        .Select(pair => pair.Key)
        .ToArray()
        ?? [];

    public IEnumerator<TKey> GetEnumerator() => m_view.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => m_view.Keys.GetEnumerator();

}
