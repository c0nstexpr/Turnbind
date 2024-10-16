using ObservableCollections;

namespace Turnbind.Helper;

public static partial class ObservableDictionaryExt
{
    public static ISynchronizedView<KeyValuePair<TKey, TValue>, U> CreateCollectionView<TKey, TValue, U>(
        this IReadOnlyObservableDictionary<TKey, TValue> dic,
        Func<KeyValuePair<TKey, TValue>, U> selector
    ) where TKey : notnull => new CollectionView<TKey, TValue, U>(dic, selector);
}
