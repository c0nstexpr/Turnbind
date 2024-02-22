namespace Turnbind
{
    using SharpHook;
    using SharpHook.Native;
    using SharpHook.Reactive;

    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    internal class KeyboardAction : IDisposable
    {
        record struct KeyState(bool Valid, bool Pressed);

        public static readonly IReactiveGlobalHook Hook = new ReactiveGlobalHookAdapter(new TaskPoolGlobalHook());

        readonly CompositeDisposable _disposables = [];

        readonly Subject<(KeyCode k, bool p)> _key = new();

        public KeyboardAction()
        {
            _disposables.Add(Hook.KeyPressed.Subscribe(OnKeyPress));
            _disposables.Add(Hook.KeyReleased.Subscribe(OnKeyRelease));
        }

        public IObservable<bool> SubscribeKeys(IReadOnlyList<KeyCode> keys)
        {
            var dic = keys.Select((k, index) => new KeyValuePair<KeyCode, int>(k, index))
                .ToDictionary();
            var nextKey = 0;
            var count = keys.Count;

            Debug.Assert(count > 0);
            Debug.Assert(keys.Distinct().Count() == count);

            return _key.Select(
                tuple =>
                {
                    var (k, p) = tuple;

                    if (p && k == keys[nextKey])
                    {
                        ++nextKey;
                        return new KeyState(nextKey == count, true);
                    }

                    if (dic.TryGetValue(k, out var i) && i < nextKey)
                        nextKey = i;

                    return new(nextKey == count - 1, false);
                }
            )
                .Where(s => s.Valid)
                .Select(s => s.Pressed);
        }

        void OnKeyRelease(KeyboardHookEventArgs args) => _key.OnNext((args.Data.KeyCode, false));

        void OnKeyPress(KeyboardHookEventArgs args) => _key.OnNext((args.Data.KeyCode, true));

        public void Dispose() => _disposables.Dispose();
    }
}
