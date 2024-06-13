using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using Turnbind.Model;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
namespace Turnbind.Action;

public sealed partial class InputAction : IDisposable
{
    readonly ILogger<InputAction> m_logger = App.GetRequiredService<ILogger<InputAction>>();

    readonly FreeLibrarySafeHandle m_moduleHandle = PInvoke.GetModuleHandle(typeof(InputAction).Module.Name);

    readonly UnhookWindowsHookExSafeHandle m_keyboardHook;

    readonly UnhookWindowsHookExSafeHandle m_mouseHook;

    readonly HOOKPROC m_keyboardProc;

    readonly HOOKPROC m_mouseProc;

    SpinLock m_keyboardLock = new();

    SpinLock m_mouseLock = new();

    public InputAction()
    {
        m_keyboardProc = OnKeyboard;
        m_mouseProc = OnMouse;

        m_keyboardHook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, m_keyboardProc, m_moduleHandle, 0);
        m_mouseHook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL, m_mouseProc, m_moduleHandle, 0);
    }

    #region Keyboard
    public record struct KeyState(InputKey Key, bool Pressed);

    public KeyState LatestKeyState { get; private set; }

    readonly Dictionary<InputKey, bool> m_pressedKeys =
        Enum.GetValues<InputKey>().Distinct().ToDictionary(k => k, _ => false);

    readonly Subject<KeyState> m_keysInput = new();

    public IObservable<KeyState> KeyboardInput => m_keysInput;

    static readonly IReadOnlyList<InputKey> m_shiftKeys = new InputKey[] {
        InputKey.Shift,
        InputKey.ShiftKey,
        InputKey.LShiftKey,
        InputKey.RShiftKey,
        InputKey.Menu,
        InputKey.Alt,
        InputKey.LMenu,
        InputKey.RMenu,
    }.Distinct().ToArray();

    public static readonly IReadOnlySet<InputKey> ModifierKeys = new InputKey[] {
        InputKey.LControlKey,
        InputKey.RControlKey,
        InputKey.LWin,
        InputKey.RWin
    }.Append(m_shiftKeys).Distinct().ToHashSet();

    void UpdateModifiers(InputKey currentKey)
    {
        foreach (var k in from k in ModifierKeys where currentKey != k && PInvoke.GetKeyState((int)k) >= 0 && m_pressedKeys[k] select k)
        {
            m_logger.LogInformation("Modifier key released: {modifier}", k);

            m_pressedKeys[k] = false;
            m_keysInput.OnNext(new(k, false));
        }
    }

    LRESULT OnKeyboard(int code, WPARAM wParam, LPARAM lParam)
    {
        var e = wParam.Value;
        var kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)!;

        if (e is PInvoke.WM_KEYDOWN or PInvoke.WM_SYSKEYDOWN) OnKey(kbd, true);
        else if (e is PInvoke.WM_KEYUP or PInvoke.WM_SYSKEYUP) OnKey(kbd, false);

        return PInvoke.CallNextHookEx(HHOOK.Null, code, wParam, lParam);
    }

    void OnKey(KBDLLHOOKSTRUCT keyPtr, bool pressed)
    {
        var lockTaken = false;

        try
        {
            m_keyboardLock.Enter(ref lockTaken);

            if (!lockTaken || m_keysInput.IsDisposed) return;

            var input = (InputKey)keyPtr.vkCode;

            UpdateModifiers(input); // Modifier key released event need to be handled manually

            LatestKeyState = new(input, pressed);

            if (m_pressedKeys[input] == pressed) return;

            m_logger.LogInformation(
                "Key {pressedStr} {input}",
                pressed ? "pressed" : "released",
                input
            );

            m_keysInput.OnNext(LatestKeyState);
            m_pressedKeys[input] = pressed;
        }
        finally
        {
            if (lockTaken) m_keyboardLock.Exit();
        }
    }

    public IObservable<bool> SubscribeKeys(InputKeys keys)
    {
        var count = keys.Count;

        if (count == 0) return Observable.Empty<bool>();

        var next = 0;

        return m_keysInput.Where(
            state =>
            {
                var (key, pressed) = state;

                if (!keys.TryGetValue(key, out var i)) return false;

                if (pressed)
                {
                    if (next == count || i != next) return false;

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

    #endregion

    #region Mouse

    readonly BehaviorSubject<MSLLHOOKSTRUCT> m_mouseMove = new(default);

    public Point Point => m_mouseMove.Value.pt;

    public IObservable<MSLLHOOKSTRUCT> MouseMoveRaw => m_mouseMove;

    public IObservable<Point> MouseMove => m_mouseMove.Select(m => m.pt);

    LRESULT OnMouse(int code, WPARAM wParam, LPARAM lParam)
    {
        if (wParam != PInvoke.WM_MOUSEMOVE) return PInvoke.CallNextHookEx(HHOOK.Null, code, wParam, lParam);

        var lockTaken = false;

        try
        {
            m_mouseLock.Enter(ref lockTaken);
            m_mouseMove.OnNext(Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam));
        }
        finally
        {
            if (lockTaken) m_mouseLock.Exit();
        }

        return PInvoke.CallNextHookEx(HHOOK.Null, code, wParam, lParam);
    }

    #endregion

    public void Dispose()
    {
        m_keyboardHook.Dispose();
        m_mouseHook.Dispose();
        m_moduleHandle.Dispose();
    }
}