using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using MoreLinq;

using Turnbind.Model;
namespace Turnbind.Action;

public sealed partial class InputAction : IDisposable
{
    readonly ILogger<InputAction> m_logger = App.GetRequiredService<ILogger<InputAction>>();

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

    readonly nint m_mouseHook;

    readonly HookProc m_keyboardProc;

    readonly HookProc m_mouseProc;

    SpinLock m_spinLock = new();

    public InputAction()
    {
        m_keyboardProc = OnKeyboard;
        m_mouseProc = OnMouse;

        var hMod = GetModuleHandle((string?)Process.GetCurrentProcess().MainModule!.ModuleName);

        m_keyboardHook = SetWindowsHookEx(13, m_keyboardProc, hMod, 0);
        m_mouseHook = SetWindowsHookEx(14, m_mouseProc, hMod, 0);
    }

    #region Keyboard
    public record struct KeyState(InputKey Key, bool Pressed);

    public KeyState LatestKeyState { get; private set; }

    readonly Dictionary<InputKey, bool> m_pressedKeys =
        Enum.GetValues<InputKey>().Distinct().ToDictionary(k => k, _ => false);

    readonly Subject<KeyState> m_keysInput = new();

    public IObservable<KeyState> KeysInput => m_keysInput;

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

    public static readonly IReadOnlyList<InputKey> ModifierKeys = new InputKey[] {
        InputKey.LControlKey,
        InputKey.RControlKey,
        InputKey.LWin,
        InputKey.RWin
    }.Append(m_shiftKeys).Distinct().ToArray();

    enum KeyEvent
    {
        WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_SYSKEYUP = 0x104,
        WM_SYSKEYDOWN = 0x105
    }

    [StructLayout(LayoutKind.Sequential)]
    class KBDLLHOOKSTRUCT
    {
        public InputKey VkCode;
        public int ScanCode;
        public int Flags;
        public uint Time;
        public nuint DwExtraInfo;
    }

    void UpdateModifier(InputKey modifier)
    {
        m_logger.LogInformation("Modifier key released: {modifier}", modifier);

        m_pressedKeys[modifier] = false;
        m_keysInput.OnNext(new(modifier, false));
    }

    void UpdateModifiers(InputKey currentKey) =>
        ModifierKeys.Where(k => currentKey != k && GetKeyState(k) >= 0 && m_pressedKeys[k])
            .ForEach(UpdateModifier);

    nint OnKeyboard(int nCode, nint wParam, nint lParam)
    {
        var e = (KeyEvent)wParam;
        var kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)!;

        if (e is KeyEvent.WM_KEYDOWN or KeyEvent.WM_SYSKEYDOWN) OnKey(kbd, true);
        else if (e is KeyEvent.WM_KEYUP or KeyEvent.WM_SYSKEYUP) OnKey(kbd, false);

        return CallNextHookEx(nint.Zero, nCode, wParam, lParam);
    }

    void OnKey(KBDLLHOOKSTRUCT keyPtr, bool pressed)
    {
        var lockTaken = false;

        try
        {
            m_spinLock.TryEnter(0, ref lockTaken);

            if (!lockTaken || m_keysInput.IsDisposed) return;

            var input = keyPtr.VkCode;

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
            if (lockTaken) m_spinLock.Exit();
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

    readonly Subject<Point> m_mouseMove = new();

    public IObservable<Point> MouseMove => m_mouseMove;

    [StructLayout(LayoutKind.Sequential)]
    public class Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    class MSLLHOOKSTRUCT
    {
        public Point? Pt;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public uint DwExtraInfo;
    }

    nint OnMouse(int nCode, nint wParam, nint lParam)
    {
        if (wParam != 0x0200) return CallNextHookEx(nint.Zero, nCode, wParam, lParam);

        m_mouseMove.OnNext(Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam)!.Pt!);

        return CallNextHookEx(nint.Zero, nCode, wParam, lParam);
    }

    #endregion

    public void Dispose()
    {
        UnhookWindowsHookEx(m_keyboardHook);
        UnhookWindowsHookEx(m_mouseHook);

        {
            var lockTaken = false;
            m_spinLock.TryEnter(ref lockTaken);

            if (!lockTaken) return;
        }

        m_keysInput.Dispose();
        m_spinLock.Exit();
    }
}