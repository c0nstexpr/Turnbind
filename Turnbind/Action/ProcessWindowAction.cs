using Serilog;

using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

namespace Turnbind;

internal partial class ProcessWindowAction : IDisposable
{
    static readonly ILogger m_logger = Util.GetLogger<ProcessWindowAction>();

    public string? ProcessName { get; set; }

    public Process? Process
    {
        get
        {
            if (m_process is not null) return m_process;

            if (ProcessName is null) return null;

            var candidates = Process.GetProcessesByName(ProcessName);

            if (candidates.Length == 0)
            {
                m_logger.Warning("No processes found for {ProcessName}", ProcessName);
                return null;
            }

            if (candidates.Length > 1)
            {
                m_logger.Warning("Multiple processes found for {ProcessName}", ProcessName);
                return null;
            }

            m_process = candidates[0];

            return m_process;
        }

        private set => m_process = value;
    }

    public IntPtr? WindowHandle => Process?.MainWindowHandle;

    readonly Subject<bool> _focused = new();

    public IObservable<bool> Focused => _focused;

    readonly IntPtr m_focusedHook;
    readonly WinEventDelegate _focusedCallback;
    readonly WinEventDelegate _destroyedCallback;
    readonly IntPtr m_destroyedHook;
    Process? m_process = null;

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

        _focusedCallback = (_, _, win, _, _, _, _) =>
        {
            if(WindowHandle is null) return;

            if (win == WindowHandle)
            {
                m_logger.Information("Window focused");
                _focused.OnNext(true);
            }
            else
            {
                m_logger.Information("Window lost focuse");
                _focused.OnNext(false);
            }
        };

        _destroyedCallback = (_, _, win, _, _, _, _) =>
        {
            if (WindowHandle != win) return;

            m_logger.Information("Window destroyed");
            _focused.OnNext(false);

            Process = null;
        };

        m_focusedHook = SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero,
            _focusedCallback,
            0,
            0,
            WINEVENT_OUTOFCONTEXT
        );

        m_destroyedHook = SetWinEventHook(
            EVENT_OBJECT_DESTROY,
            EVENT_OBJECT_DESTROY,
            IntPtr.Zero,
            _destroyedCallback,
            0,
            0,
            WINEVENT_OUTOFCONTEXT
        );
    }

    public void Dispose()
    {
        UnhookWinEvent(m_focusedHook);
        UnhookWinEvent(m_destroyedHook);
    }
}
