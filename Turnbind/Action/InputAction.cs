using SharpHook;
using SharpHook.Reactive;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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

        m_disposables.Add(Hook.KeyPressed.Subscribe(OnKeyPress));
        m_disposables.Add(Hook.MousePressed.Subscribe(OnMousePress));
        m_disposables.Add(Hook.KeyReleased.Subscribe(OnKeyRelease));
        m_disposables.Add(Hook.MouseReleased.Subscribe(OnMouseRelease));
        // m_disposables.Add(Hook.MouseWheel.Subscribe(OnMouseWheel));
        m_disposables.Add(Hook.RunAsync().Subscribe());
    }

    public IObservable<bool> SubscribeKeys(InputKeys keys)
    {
        var count = keys.Count;

        if (count == 0) return Observable.Empty<bool>();

        var dic = keys.Select((k, index) => new KeyValuePair<InputKey, int>(k, index))
            .ToDictionary();

        var nextKeyI = 0;

        return m_input.Select(
            state =>
            {
                var i = -1;
                dic.TryGetValue(state.Key, out i);
                return (state, i);
            }
        )
            .Where(
                 tuple =>
                {
                    var (state, index) = tuple;

                    if (index == -1) return false;

                    if (state.Pressed)
                    {
                        if (nextKeyI == count)
                            return false;
                        else
                        {
                            if(index == 
                            nextKeyI++;
                        }
                    }

                    return true;
                }
            )
            .Select(
                tuple =>
                {
                    var (state, i) = tuple;
                    var (k, p) = state;

                    if (p)
                    {
                        nextKeyI++;
                        return nextKeyI == count;
                    }

                    nextKeyI = i;
                    return new(true, false);
                }
            )
            .Select(s => s.Pressed);
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
        Hook.Dispose();
        m_taskPoolGlobalHook.Dispose();
        m_disposables.Dispose();
        m_input.Dispose();
    }
}
