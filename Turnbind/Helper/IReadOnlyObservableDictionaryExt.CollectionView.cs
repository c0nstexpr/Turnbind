using System.Collections;
using System.Collections.Specialized;

using LanguageExt.Pretty;

using ObservableCollections;

namespace Turnbind.Helper;

public static partial class ObservableDictionaryExt
{
    sealed class CollectionView<TKey, TValue, U> : ISynchronizedView<KeyValuePair<TKey, TValue>, U> where TKey : notnull
    {
        readonly Key2IndexCollection<TKey> m_key2Index;

        readonly ISynchronizedView<KeyValuePair<TKey, TValue>, U> m_view;

        public object SyncRoot => m_view.SyncRoot;

        public int Count => m_view.Count;

        public event NotifyViewChangedEventHandler<KeyValuePair<TKey, TValue>, U>? ViewChanged
        {
            add => m_view.ViewChanged += value;

            remove => m_view.ViewChanged -= value;
        }

        public event Action<NotifyCollectionChangedAction>? CollectionStateChanged
        {
            add => m_view.CollectionStateChanged += value;

            remove => m_view.CollectionStateChanged -= value;
        }

        public event Action<RejectedViewChangedAction, int, int>? RejectedViewChanged
        {
            add => m_view.RejectedViewChanged += value;

            remove => m_view.RejectedViewChanged -= value;
        }

        public ISynchronizedViewFilter<KeyValuePair<TKey, TValue>> Filter => m_view.Filter;

        public IEnumerable<(KeyValuePair<TKey, TValue> Value, U View)> Filtered => m_view.Filtered;

        public IEnumerable<(KeyValuePair<TKey, TValue> Value, U View)> Unfiltered => m_view.Filtered;

        public int UnfilteredCount => m_view.UnfilteredCount;

        public CollectionView(IReadOnlyObservableDictionary<TKey, TValue> dic, Func<KeyValuePair<TKey, TValue>, U> selector)
        {
            m_key2Index = new(dic.Keys);
            m_view = dic.CreateView(selector);

            m_view.ViewChanged += OnChanged;
        }

        void OnChanged(in SynchronizedViewChangedEventArgs<KeyValuePair<TKey, TValue>, U> e)
        {
            if (!e.IsSingleItem) throw new InvalidOperationException($"Expected single item, got {e.Action}");

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    m_key2Index.Clear();

                    foreach (var (pair, _) in m_view.Unfiltered)
                        m_key2Index.Add(pair.Key);

                    // TODO
                    break;

                case NotifyCollectionChangedAction.Add:
                    var item = e.NewItem;
                    var value = item.Value;
                    var key = value.Key;

                    if (!m_key2Index.Add(key)) break;

                    var i = m_key2Index[key];

                    // TODO
                    break;

                case NotifyCollectionChangedAction.Remove:
                    item = e.OldItem;
                    value = item.Value;
                    key = value.Key;

                    if (!m_key2Index.TryGetValue(key, out i)) break;

                    m_key2Index.Remove(key);

                    // TODO
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var oldItem = e.OldItem;
                    var oldKey = oldItem.Value.Key;

                    item = e.NewItem;
                    value = item.Value;
                    key = value.Key;

                    if (!oldKey.Equals(key)) throw new InvalidOperationException($"Key mismatch: {oldKey} != {key}");

                    if (!m_key2Index.TryGetValue(oldKey, out i)) break;

                    // TODO
                    break;
            }

        }

        public void Dispose()
        {
            m_view.ViewChanged -= OnChanged;
            m_view.Dispose();
        }

        public void AttachFilter(ISynchronizedViewFilter<KeyValuePair<TKey, TValue>> filter) => m_view.AttachFilter(filter);

        public void ResetFilter() => m_view.ResetFilter();

        public ISynchronizedViewList<U> ToViewList() => m_view.ToViewList();

        class NCCSVL : NotifyCollectionChangedSynchronizedViewList<U>
        {
            CollectionView<TKey, TValue, U> m_source;
        }

        public NotifyCollectionChangedSynchronizedViewList<U> ToNotifyCollectionChanged(ICollectionEventDispatcher? collectionEventDispatcher)
        {
            var view = m_view.ToNotifyCollectionChanged(collectionEventDispatcher);
        }

        public NotifyCollectionChangedSynchronizedViewList<U> ToNotifyCollectionChanged()
        {
        }


        public IEnumerator<U> GetEnumerator() => m_view.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_view.GetEnumerator();
    }
}
