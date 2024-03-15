using System.Reactive.Disposables;

using MoreLinq;

namespace Turnbind.Helper;

public static class DisposableExt
{
    public static void Add(this CompositeDisposable disposable, System.Action action) =>
        disposable.Add(Disposable.Create(action));

    public static void AddRange(this CompositeDisposable disposable, IEnumerable<IDisposable> enumerable) =>
        enumerable.ForEach(disposable.Add);

    public static void AddRange(this CompositeDisposable disposable, params IDisposable[] disposables) =>
        disposable.AddRange((IEnumerable<IDisposable>)disposables);

    public static void AddRange(this CompositeDisposable disposable, IEnumerable<System.Action> actions) =>
        actions.ForEach(disposable.Add);

    public static void AddRange(this CompositeDisposable disposable, params System.Action[] actions) =>
        disposable.AddRange((IEnumerable<System.Action>)actions);
}
