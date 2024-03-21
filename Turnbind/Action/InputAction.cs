using SharpHook;
using SharpHook.Reactive;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Turnbind.Helper;
using Turnbind.Model;

namespace Turnbind.Action;

public sealed class InputAction : IDisposable
{
    public record struct KeyState(InputKey Key, bool Pressed);

    readonly TaskPoolGlobalHook m_taskPoolGlobalHook = new();

    public readonly IReactiveGlobalHook Hook;

    public KeyState LatestKeyState { get; private set; }

    readonly CompositeDisposable m_disposables = [];

    readonly Dictionary<InputKey, bool> m_pressedKeys =
        Enum.GetValues<InputKey>().ToDictionary(k => k, _ => false);

    readonly Subject<KeyState> m_input = new();

    public IObservable<KeyState> Input => m_input;

    public InputAction()
    {
        Hook = new ReactiveGlobalHookAdapter(m_taskPoolGlobalHook);

        m_disposables.AddRange(
            Hook.KeyPressed.Subscribe(OnKeyPress),
            Hook.MousePressed.Subscribe(OnMousePress),
            Hook.KeyReleased.Subscribe(OnKeyRelease),
            Hook.MouseReleased.Subscribe(OnMouseRelease),
            Hook.RunAsync().Subscribe()
            // Hook.MouseWheel.Subscribe(OnMouseWheel)
        );
    }

    public IObservable<bool> SubscribeKeys(InputKeys keys)
    {
        var count = keys.Count;

        if (count == 0) return Observable.Empty<bool>();

        var next = 0;

        return m_input.Where(
            state =>
            {
                var (key, pressed) = state;

                if (!keys.TryGetValue(key, out var i)) return false;

                if (pressed)
                {
                    if(next == count || i != next) return false;

                    ++next;

                    return next == count;
                }
                
                if (i >= next) return false;

                var isDeactive = next == count;

                next = i;

                return isDeactive;                
            }
        )
            .Select(state => state.Pressed);
    }

    void OnKeyRelease(KeyboardHookEventArgs args)
    {
        var input = args.Data.KeyCode.ToInputKey();
        LatestKeyState = new(input, false);
        m_input.OnNext(LatestKeyState);
        m_pressedKeys[input] = false;
    }

    void OnKeyPress(KeyboardHookEventArgs args)
    {
        var input = args.Data.KeyCode.ToInputKey();

        LatestKeyState = new(input, true);

        if (m_pressedKeys[input]) return;

        m_input.OnNext(LatestKeyState);
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
        m_taskPoolGlobalHook.Dispose();
        m_input.Dispose();
    }
}
