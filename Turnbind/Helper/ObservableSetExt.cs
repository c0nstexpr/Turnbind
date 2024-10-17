using ObservableCollections;

namespace Turnbind.Helper;

public static partial class ObservableSetExt
{
    public static ISynchronizedView<TKey, U> CreateCollectionView<TSet, TKey, U>(this TSet set, Func<TKey, U> selector)
        where TSet : IObservableCollection<TKey>, IReadOnlySet<TKey>
        where TKey : notnull => 
        new CollectionView<TSet, TKey, U>(set, selector);
}
