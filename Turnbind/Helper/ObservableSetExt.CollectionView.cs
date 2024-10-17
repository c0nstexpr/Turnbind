using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

using MoreLinq;

using ObservableCollections;

using SpanLinq;

namespace Turnbind.Helper;

public static partial class ObservableSetExt
{
    sealed class CollectionView<TSet, TKey, U> : ISynchronizedView<TKey, U>
        where TSet : IObservableCollection<TKey>, IReadOnlySet<TKey>
        where TKey : notnull
    {
        public Func<TKey, U> Selector { get; }

        readonly Key2IndexCollection<TKey> m_key2Index;

        readonly ObservableList<TKey> m_collection;

        readonly ISynchronizedView<TKey, U> m_view;

        public object SyncRoot => m_collection.SyncRoot;

        public int Count => m_collection.Count;

        public event NotifyViewChangedEventHandler<TKey, U>? ViewChanged
        {
            add => m_view.ViewChanged += value;

            remove => m_view.ViewChanged -= value;
        }

        public event Action<NotifyCollectionChangedAction>? CollectionStateChanged
        {
            add => m_view.CollectionStateChanged += value;

            remove => m_view.CollectionStateChanged -= value;
        }

        public TSet Set { get; }

        public ISynchronizedViewFilter<TKey> Filter => m_view.Filter;

        public IEnumerable<(TKey Value, U View)> Filtered => m_view.Filtered;

        public IEnumerable<(TKey Value, U View)> Unfiltered => m_view.Filtered;

        public int UnfilteredCount => m_view.UnfilteredCount;

        public CollectionView(TSet set, Func<TKey, U> selector)
        {
            Selector = selector;
            Set = set;
            m_key2Index = new(set);
            m_collection = new(set);
            m_view = m_collection.CreateView(selector);
            set.CollectionChanged += OnDicChanged;
        }

        bool OnRemove(TKey key)
        {
            if (m_key2Index.TryGetValue(key, out var i)) return false;

            m_key2Index.Remove(key);
            m_collection.RemoveAt(i);

            return true;
        }

        bool OnAdd(TKey key)
        {
            if (!m_key2Index.Add(key)) return false;

            m_collection[m_key2Index[key]] = key;
            return true;
        }

        void OnReplace(TKey oldKey, TKey key)
        {
            Debug.Assert(oldKey.Equals(key));

            if (!m_key2Index.TryGetValue(oldKey, out var i)) return;

            m_collection[m_key2Index[oldKey]] = key;
        }

        void OnReset()
        {
            m_key2Index.Clear();
            m_collection.Clear();

            foreach (var key in Set)
            {
                m_key2Index.Add(key);
                m_collection.Add(key);
            }
        }

        void OnDicChanged(in NotifyCollectionChangedEventArgs<TKey> e)
        {
            var action = e.Action;

            if (action == NotifyCollectionChangedAction.Reset)
            {
                OnReset();
                return;
            }

            if (e.IsSingleItem)
            {
                switch (action)
                {
                    case NotifyCollectionChangedAction.Add:
                        OnAdd(e.NewItem);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        OnRemove(e.OldItem);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        OnReplace(e.OldItem, e.NewItem);
                        break;
                }

                return;
            }

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems.ForEach(k => OnAdd(k));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.ForEach(k => OnRemove(k));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var oldItems = e.OldItems;
                    var newItems = e.NewItems;

                    Debug.Assert(newItems.Length == oldItems.Length);

                    for (var i = 0; i < oldItems.Length; ++i) OnReplace(oldItems[i], newItems[i]);
                    break;
            }
        }

        public void Dispose() => Set.CollectionChanged -= OnDicChanged;

        public void AttachFilter(ISynchronizedViewFilter<TKey> filter) => m_view.AttachFilter(filter);

        public void ResetFilter() => m_view.ResetFilter();

        public ISynchronizedViewList<U> ToViewList() => m_view.ToViewList();

        public INotifyCollectionChangedSynchronizedViewList<U> ToNotifyCollectionChanged() => m_view.ToNotifyCollectionChanged();

        public INotifyCollectionChangedSynchronizedViewList<U> ToNotifyCollectionChanged(ICollectionEventDispatcher? collectionEventDispatcher) => m_view.ToNotifyCollectionChanged(collectionEventDispatcher);

        public IEnumerator<U> GetEnumerator() => m_view.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_view).GetEnumerator();
    }
}
