using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace Turnbind.Helper;

partial class ObservableDictionaryListView<TKey, TValue>
{
    public sealed class ValueCollectionChanged : INotifyCollectionChanged, IReadOnlyList<TValue>, IDisposable
    {
        readonly ObservableDictionaryListView<TKey, TValue> m_view;

        readonly IDisposable m_disposable;

        public int Count => m_view.Count;

        public bool IsReadOnly => true;

        public TValue this[int i] => m_view.GetValueAtIndex(i);

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        internal ValueCollectionChanged(ObservableDictionaryListView<TKey, TValue> view)
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

                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(
                        this,
                        new(
                            NotifyCollectionChangedAction.Replace,
                            oldItems,
                            newItems,
                            e.NewStartingIndex
                        )
                    );
                    break;

                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        static IEnumerable<TValue>? Select(IList? list) => list?.Cast<KeyValuePair<TKey, TValue>>()
                                        .Select(pair => pair.Value);
        public IEnumerator<TValue> GetEnumerator() => m_view.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_view.Values.GetEnumerator();

        public void Dispose() => m_disposable.Dispose();
    }
}
