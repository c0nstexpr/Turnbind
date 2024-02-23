namespace Turnbind.Action
{
    using SharpHook;
    using SharpHook.Native;
    using SharpHook.Reactive;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Turnbind.Model;

    public class InputAction : IDisposable
    {
        public record struct KeyState(InputKey Key, bool Pressed);

        public static readonly IReactiveGlobalHook Hook = new ReactiveGlobalHookAdapter(new TaskPoolGlobalHook());

        readonly CompositeDisposable _disposables = [];

        readonly Subject<KeyState> _input = new();

        public IObservable<KeyState> Input => _input;

        public InputAction()
        {
            _disposables.Add(Hook.KeyPressed.Subscribe(OnKeyPress));
            _disposables.Add(Hook.MousePressed.Subscribe(OnMousePress));
            _disposables.Add(Hook.KeyReleased.Subscribe(OnKeyRelease));
            _disposables.Add(Hook.MouseReleased.Subscribe(OnMouseRelease));
            _disposables.Add(Hook.MouseWheel.Subscribe(OnMouseWheel));
        }

        record struct KeyValidation(bool Valid, bool Pressed);

        public IObservable<bool> SubscribeKeys(IReadOnlyList<InputKey> keys)
        {
            var dic = keys.Select((k, index) => new KeyValuePair<InputKey, int>(k, index))
                .ToDictionary();
            var nextKey = 0;
            var count = keys.Count;

            Debug.Assert(count > 0);
            Debug.Assert(keys.Distinct().Count() == count);

            return _input.Select(
                state =>
                {
                    var (k, p) = state;

                    if (p && k == keys[nextKey])
                    {
                        ++nextKey;
                        return new KeyValidation(nextKey == count, true);
                    }

                    if (dic.TryGetValue(k, out var i) && i < nextKey)
                        nextKey = i;

                    return new(nextKey == count - 1, false);
                }
            )
                .Where(s => s.Valid)
                .Select(s => s.Pressed);
        }

        void OnKeyRelease(KeyboardHookEventArgs args) => _input.OnNext(new(args.Data.KeyCode.ToInputKey(), false));

        void OnKeyPress(KeyboardHookEventArgs args) => _input.OnNext(new(args.Data.KeyCode.ToInputKey(), true));

        void OnMousePress(MouseHookEventArgs args) => _input.OnNext(new(args.Data.Button.ToInputKey(), true));

        void OnMouseRelease(MouseHookEventArgs args) => _input.OnNext(new(args.Data.Button.ToInputKey(), false));

        void OnMouseWheel(MouseWheelHookEventArgs args)
        {
            var data = args.Data;

            if (data.Direction != MouseWheelScrollDirection.Vertical) return;

            var isUp = args.Data.Rotation > 0;

            _input.OnNext(new(InputKey.MouseWheelUp, isUp));
            _input.OnNext(new(InputKey.MouseWheelDown, !isUp));
        }

        public void Dispose() => _disposables.Dispose();
    }
}
