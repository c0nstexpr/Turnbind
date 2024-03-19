using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace Turnbind.Helper;

partial class ObservableDictionaryListView<TKey, TValue>
{
    public sealed class KeyCollectionChanged : INotifyCollectionChanged, IReadOnlyList<TKey>, IDisposable
    {
        readonly ObservableDictionaryListView<TKey, TValue> m_view;

        readonly IDisposable m_disposable;

        public int Count => m_view.Count;

        public bool IsReadOnly => true;

        public TKey this[int i] => m_view.GetKeyAtIndex(i);

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        internal KeyCollectionChanged(ObservableDictionaryListView<TKey, TValue> view)
        {
            m_view = view;
            view.CollectionChanged += OnCollectionChanged;
            m_disposable = Disposable.Create(() => view.CollectionChanged -= OnCollectionChanged);
        }

        void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = Select(e.NewItems);
            var oldItems = Select(e.OldItems);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    CollectionChanged?.Invoke(
                        this,
                        new(
                            NotifyCollectionChangedAction.Add,
                            newItems,
                            e.NewStartingIndex
                        )
                    );
                    break;

                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(
                        this,
                        new(
                            NotifyCollectionChangedAction.Remove,
                            oldItems,
                            e.OldStartingIndex
                        )
                    );
                    break;            

                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        static IEnumerable<TKey>? Select(IList? list) => list?.Cast<KeyValuePair<TKey, TValue>>()
                                        .Select(pair => pair.Key);
        public IEnumerator<TKey> GetEnumerator() => m_view.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_view.Keys.GetEnumerator();

        public void Dispose() => m_disposable.Dispose();

    }
}
