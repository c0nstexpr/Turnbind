using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

using Serilog;

using Turnbind.Helper;

namespace Turnbind;

partial class ProcessWindowAction : IDisposable
{
    public string? ProcessName { get; set; }

    public IntPtr? WindowHandle
    {
        get
        {
            if (ProcessName is null) return null;

            var candidates = Process.GetProcessesByName(ProcessName);

            if (candidates.Length == 0)
            {
                Log.Logger.WithSourceInfo().Information("No processes found for {ProcessName}", ProcessName);
                return null;
            }

            if (candidates.Length > 1)
            {
                Log.Logger.WithSourceInfo().Information("Multiple processes found for {ProcessName}", ProcessName);
                return null;
            }

            return candidates[0].MainWindowHandle;
        }
    }

    readonly BehaviorSubject<bool> m_focused = new(false);

    public BehaviorObservable<bool> Focused { get; }

    readonly IntPtr m_focusedHook;
    readonly WinEventDelegate m_focusedCallback;
    readonly WinEventDelegate m_destroyedCallback;
    readonly IntPtr m_destroyedHook;

    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [LibraryImport("user32.dll")]
    private static partial IntPtr SetWinEventHook(
        uint eventMin,
        uint eventMax,
        IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags
    );

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWinEvent(IntPtr hWinEventHook);

    public ProcessWindowAction()
    {
        const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        const uint EVENT_OBJECT_DESTROY = 0x8001;
        const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        Focused = m_focused.AsObservable();

        m_focusedCallback = (_, _, win, _, _, _, _) =>
        {
            if (m_focused.IsDisposed) return;

            var handle = WindowHandle;

            if (handle is null) return;

            if (win == handle && !m_focused.Value)
            {
                Log.Logger.WithSourceInfo().Information("Window focused");
                m_focused.OnNext(true);
            }
            else if(m_focused.Value)
            {
                Log.Logger.WithSourceInfo().Information("Window lost focuse");
                m_focused.OnNext(false);
            }
        };

        m_destroyedCallback = (_, _, win, _, _, _, _) =>
        {
            var handle = WindowHandle;

            if (handle is null || win != handle) return;

            Log.Logger.WithSourceInfo().Information("Window destroyed");

            if(m_focused.IsDisposed) return;

            m_focused.OnNext(false);
        };

        m_focusedHook = SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero,
            m_focusedCallback,
            0,
            0,
            WINEVENT_OUTOFCONTEXT
        );

        m_destroyedHook = SetWinEventHook(
            EVENT_OBJECT_DESTROY,
            EVENT_OBJECT_DESTROY,
            IntPtr.Zero,
            m_destroyedCallback,
            0,
            0,
            WINEVENT_OUTOFCONTEXT
        );
    }

    public void Dispose()
    {
        UnhookWinEvent(m_focusedHook);
        UnhookWinEvent(m_destroyedHook);
        m_focused.Dispose();
    }
}
