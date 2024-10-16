using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;

using LanguageExt.Pretty;

using MoreLinq;

using ObservableCollections;

using SpanLinq;

namespace Turnbind.Helper;

public static partial class ObservableDictionaryExt
{
    sealed class CollectionView<TKey, TValue, U> : ISynchronizedView<KeyValuePair<TKey, TValue>, U> where TKey : notnull
    {
        public Func<KeyValuePair<TKey, TValue>, U> Selector { get; }

        readonly Key2IndexCollection<TKey> m_key2Index;

        readonly ObservableList<KeyValuePair<TKey, TValue>> m_collection;

        readonly ISynchronizedView<KeyValuePair<TKey, TValue>, U> m_view;

        public object SyncRoot => m_collection.SyncRoot;

        public int Count => m_collection.Count;

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

        public IReadOnlyObservableDictionary<TKey, TValue> Dic { get; }

        public ISynchronizedViewFilter<KeyValuePair<TKey, TValue>> Filter => m_view.Filter;

        public IEnumerable<(KeyValuePair<TKey, TValue> Value, U View)> Filtered => m_view.Filtered;

        public IEnumerable<(KeyValuePair<TKey, TValue> Value, U View)> Unfiltered => m_view.Filtered;

        public int UnfilteredCount => m_view.UnfilteredCount;

        public CollectionView(IReadOnlyObservableDictionary<TKey, TValue> dic, Func<KeyValuePair<TKey, TValue>, U> selector)
        {
            Selector = selector;
            Dic = dic;
            m_key2Index = new(dic.Keys);
            m_collection = new(dic);
            m_view = m_collection.CreateView(selector);
            dic.CollectionChanged += OnDicChanged;
        }

        bool OnRemove(TKey key)
        {
            if (m_key2Index.TryGetValue(key, out var i)) return false;

            m_key2Index.Remove(key);
            m_collection.RemoveAt(i);

            return true;
        }

        bool OnAdd(KeyValuePair<TKey, TValue> pair)
        {
            var key = pair.Key;

            if (!m_key2Index.Add(key)) return false;

            m_collection[m_key2Index[key]] = pair;
            return true;
        }

        void OnReplace(TKey oldKey, KeyValuePair<TKey, TValue> pair)
        {
            Debug.Assert(oldKey.Equals(pair.Key));

            if (!m_key2Index.TryGetValue(oldKey, out var i)) return;

            m_collection[m_key2Index[oldKey]] = pair;
        }

        void OnReset()
        {
            m_key2Index.Clear();
            m_collection.Clear();

            foreach (var pair in Dic)
            {
                m_key2Index.Add(pair.Key);
                m_collection.Add(pair);
            }
        }

        void OnDicChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e)
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
                        OnRemove(e.OldItem.Key);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        OnReplace(e.OldItem.Key, e.NewItem);
                        break;
                }

                return;
            }

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems.ForEach(p => OnAdd(p));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.ForEach(p => OnRemove(p.Key));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems.Length; ++i) OnReplace(e.OldItems[i].Key, e.NewItems[i]);
                    break;
            }
        }

        public void Dispose() => Dic.CollectionChanged -= OnDicChanged;

        public void AttachFilter(ISynchronizedViewFilter<KeyValuePair<TKey, TValue>> filter) => m_view.AttachFilter(filter);

        public void ResetFilter() => m_view.ResetFilter();

        public ISynchronizedViewList<U> ToViewList() => m_view.ToViewList();

        public INotifyCollectionChangedSynchronizedViewList<U> ToNotifyCollectionChanged() => m_view.ToNotifyCollectionChanged();

        public INotifyCollectionChangedSynchronizedViewList<U> ToNotifyCollectionChanged(ICollectionEventDispatcher? collectionEventDispatcher) => m_view.ToNotifyCollectionChanged(collectionEventDispatcher);

        public IEnumerator<U> GetEnumerator() => m_view.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_view).GetEnumerator();
    }
}
