using SharpHook;
using SharpHook.Native;
using SharpHook.Reactive;

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Turnbind.Model;

namespace Turnbind.Action;

public class InputAction : IDisposable
{
    public record struct KeyState(InputKey Key, bool Pressed);

    public readonly IReactiveGlobalHook Hook = new ReactiveGlobalHookAdapter(new TaskPoolGlobalHook());

    readonly CompositeDisposable m_disposables = [];

    readonly Dictionary<InputKey, bool> m_pressedKeys = Enum.GetValues<InputKey>().ToDictionary(k => k, _ => false);

    readonly Subject<KeyState> m_input = new();

    public IObservable<KeyState> Input => m_input;

    public InputAction()
    {
        m_disposables.Add(Hook.KeyPressed.Subscribe(OnKeyPress));
        m_disposables.Add(Hook.MousePressed.Subscribe(OnMousePress));
        m_disposables.Add(Hook.KeyReleased.Subscribe(OnKeyRelease));
        m_disposables.Add(Hook.MouseReleased.Subscribe(OnMouseRelease));
        // m_disposables.Add(Hook.MouseWheel.Subscribe(OnMouseWheel));
        m_disposables.Add(Hook.RunAsync().Subscribe());
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

        return m_input.Where(state => nextKey == count && state.Pressed).Select(
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

                var valid = nextKey == count - 1;

                return new(valid, false);
            }
        )
            .Where(s => s.Valid)
            .Select(s => s.Pressed);
    }

    void OnKeyRelease(KeyboardHookEventArgs args)
    {
        var input = args.Data.KeyCode.ToInputKey();
        m_input.OnNext(new(input, false));
        m_pressedKeys[input] = false;
    }

    void OnKeyPress(KeyboardHookEventArgs args)
    {
        var input = args.Data.KeyCode.ToInputKey();

        if (m_pressedKeys[input] == true) return;

        m_input.OnNext(new(input, true));
        m_pressedKeys[input] = true;
    }

    void OnMousePress(MouseHookEventArgs args) => m_input.OnNext(new(args.Data.Button.ToInputKey(), true));

    void OnMouseRelease(MouseHookEventArgs args) => m_input.OnNext(new(args.Data.Button.ToInputKey(), false));

    // void OnMouseWheel(MouseWheelHookEventArgs args)
    // {
    //     var data = args.Data;

    //     if (data.Direction != MouseWheelScrollDirection.Vertical) return;

    //     var isUp = args.Data.Rotation > 0;

    //     if (m_pressedKeys[InputKey.MouseWheelUp] != isUp)
    //     { 
    //         m_input.OnNext(new(InputKey.MouseWheelUp, isUp));
    //         m_pressedKeys[InputKey.MouseWheelUp] = isUp;
    //         m_input.OnNext(new(InputKey.MouseWheelDown, !isUp));
    //         m_pressedKeys[InputKey.MouseWheelDown] = !isUp;
    //     }
    // }

    public void Dispose()
    {
        m_disposables.Dispose();
        Hook.Dispose();
    }
}
