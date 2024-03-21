using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

using Turnbind.Model;
namespace Turnbind.Action;

public sealed partial class InputAction : IDisposable
{
    public record struct KeyState(InputKey Key, bool Pressed);

    public KeyState LatestKeyState { get; private set; }

    readonly Dictionary<InputKey, bool> m_pressedKeys =
        Enum.GetValues<InputKey>().Distinct().ToDictionary(k => k, _ => false);

    readonly Subject<KeyState> m_input = new();

    public IObservable<KeyState> Input => m_input;

    delegate nint HookProc(int nCode, nint wParam, nint lParam);

    [LibraryImport(
        "kernel32",
        EntryPoint = "GetModuleHandleW",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16
    )]
    private static partial nint GetModuleHandle(string? lpModuleName);

    [LibraryImport("user32", SetLastError = true, EntryPoint = "SetWindowsHookExW")]
    private static partial nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, int dwThreadId);

    [LibraryImport("user32", SetLastError = true)]
    private static partial int UnhookWindowsHookEx(nint hhk);

    [LibraryImport("user32", SetLastError = true)]
    private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [LibraryImport("user32", SetLastError = true)]
    private static partial short GetKeyState(InputKey nVirtKey);

    readonly nint m_keyboardHook;

    readonly HookProc m_hookProc;

    public InputAction()
    {
        var hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule!.ModuleName);

        m_hookProc = OnKeyboard;
        m_keyboardHook = SetWindowsHookEx(13, m_hookProc, hMod, 0);
    }

    enum KeyEvent
    {
        WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_SYSKEYUP = 0x104,
        WM_SYSKEYDOWN = 0x105
    }

    nint OnKeyboard(int nCode, nint wParam, nint lParam)
    {
        List<InputKey> modifiers = new(10);

        if (GetKeyState(InputKey.LShiftKey) < 0 && !m_pressedKeys[InputKey.LShiftKey]) { m_pressedKeys[InputKey.LShiftKey] = true; }
        if (GetKeyState(InputKey.RShiftKey) < 0) { set_modifier_mask(MASK_SHIFT_R); }
        if (GetKeyState(InputKey.LControlKey) < 0) { set_modifier_mask(MASK_CTRL_L); }
        if (GetKeyState(InputKey.RControlKey) < 0) { set_modifier_mask(MASK_CTRL_R); }
        if (GetKeyState(InputKey.LMenu) < 0) { set_modifier_mask(MASK_ALT_L); }
        if (GetKeyState(InputKey.RMenu) < 0) { set_modifier_mask(MASK_ALT_R); }
        if (GetKeyState(InputKey.LWin) < 0) { set_modifier_mask(MASK_META_L); }
        if (GetKeyState(InputKey.RWin) < 0) { set_modifier_mask(MASK_META_R); }

        switch ((KeyEvent)wParam)
        {
            case KeyEvent.WM_KEYDOWN:
                OnKeyPress(lParam);
                break;

            case KeyEvent.WM_SYSKEYDOWN:
                OnKeyPress(lParam);
                break;

            case KeyEvent.WM_KEYUP:
                OnKeyRelease(lParam);
                break;

            case KeyEvent.WM_SYSKEYUP:
                OnKeyRelease(lParam);
                break;
        }

        return CallNextHookEx(nint.Zero, nCode, wParam, lParam);
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

    void OnKeyPress(nint keyPtr)
    {
        var input = (InputKey)Marshal.ReadInt32(keyPtr);

        LatestKeyState = new(input, true);

        if (m_pressedKeys[input]) return;

        m_input.OnNext(LatestKeyState);
        m_pressedKeys[input] = true;
    }

    void OnKeyRelease(nint keyPtr)
    {
        var input = (InputKey)Marshal.ReadInt32(keyPtr);
        LatestKeyState = new(input, false);
        m_input.OnNext(LatestKeyState);
        m_pressedKeys[input] = false;
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(m_keyboardHook);
        m_input.Dispose();
    }
}
