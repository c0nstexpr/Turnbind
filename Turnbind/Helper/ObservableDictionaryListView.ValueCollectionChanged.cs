using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace Turnbind.Helper;

partial class ObservableDictionaryListView<TKey, TValue>
{
    public sealed class ValueCollectionChanged : INotifyCollectionChanged, IReadOnlyCollection<TValue>, IDisposable
    {
        readonly ObservableDictionaryListView<TKey, TValue> m_view;

        readonly IDisposable m_disposable;

        public int Count => m_view.Count;

        public bool IsReadOnly => true;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        internal ValueCollectionChanged(ObservableDictionaryListView<TKey, TValue> view)
        {
            m_view = view;
            view.CollectionChanged += OnCollectionChanged;
            m_disposable = Disposable.Create(() => view.CollectionChanged -= OnCollectionChanged);
        }

        void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    CollectionChanged?.Invoke(
                        this,
                        new(
                            NotifyCollectionChangedAction.Add,
                            e.NewItems?.Cast<KeyValuePair<TKey, TValue>>()
                                .Select(pair => pair.Value)
                                .ToList(),
                            e.NewStartingIndex
                        )
                    );
                    break;

                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(
                        this,
                        new(
                            NotifyCollectionChangedAction.Add,
                            e.OldItems?.Cast<KeyValuePair<TKey, TValue>>()
                                .Select(pair => pair.Value)
                                .ToList(),
                            e.OldStartingIndex
                        )
                    );
                    break;

                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(
                        this,
                        new(
                            NotifyCollectionChangedAction.Replace,
                            e.NewItems?.Cast<KeyValuePair<TKey, TValue>>()
                                .Select(pair => pair.Value)
                                .ToList(),
                            e.NewStartingIndex
                        )
                    );
                    break;

                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        public IEnumerator<TValue> GetEnumerator() => m_view.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_view.Values.GetEnumerator();

        public void Dispose() => m_disposable.Dispose();
    }
}
