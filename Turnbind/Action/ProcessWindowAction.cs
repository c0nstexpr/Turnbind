using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using Turnbind.Helper;

namespace Turnbind;

partial class ProcessWindowAction : IDisposable
{
    readonly ILogger<ProcessWindowAction> m_log = App.GetRequiredService<ILogger<ProcessWindowAction>>();

    public string? ProcessName { get; set; }

    readonly Dictionary<nint, Dictionary<int, Process>> m_processes = [];

    readonly BehaviorSubject<bool> m_focused = new(false);

    public BehaviorObservable<bool> Focused { get; }

    readonly nint m_focusedHook;
    readonly WinEventDelegate m_focusedCallback;

    delegate void WinEventDelegate(
        nint hWinEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime
    );

    [LibraryImport("user32")]
    private static partial nint SetWinEventHook(
        uint eventMin,
        uint eventMax,
        nint hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags
    );

    [LibraryImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWinEvent(nint hWinEventHook);

    SpinLock m_spinLock = new();

    public ProcessWindowAction()
    {
        const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

        Focused = m_focused.AsObservable();
        m_focusedCallback = OnFocused;

        m_focusedHook = SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            0,
            m_focusedCallback,
            0,
            0,
            0
        );
    }

    void OnFocused(
        nint hWinEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime
    )
    {
        var lockTaken = false;

        try
        {
            m_spinLock.TryEnter(0, ref lockTaken);

            if (!lockTaken || m_focused.IsDisposed) return;

            var focusd = m_focused.Value;
            if (ContainsWin(hwnd) && !focusd)
            {
                m_log.LogInformation("Window focused");
                m_focused.OnNext(true);
            }
            else if (focusd)
            {
                m_log.LogInformation("Window lost focuse");
                m_focused.OnNext(false);
            }
        }
        finally
        {
            if (lockTaken) m_spinLock.Exit(false);
        }
    }

    bool ContainsWin(nint hwnd)
    {
        if (m_processes.ContainsKey(hwnd)) return true;

        foreach (var p in Process.GetProcessesByName(ProcessName))
        {
            var handle = p.MainWindowHandle;

            if (!m_processes.TryGetValue(handle, out var processDic))
            {
                processDic = new(3);
                m_processes.Add(handle, processDic);
            }

            processDic.TryAdd(p.Id, p);

            EventHandler onExit = default!;

            onExit = (s, e) =>
            {
                processDic.Remove(p.Id);

                if (processDic.Count == 0) m_processes.Remove(handle);

                p.Exited -= onExit;
            };

            p.Exited += onExit;
        }

        return m_processes.ContainsKey(hwnd);
    }

    public void Dispose()
    {
        UnhookWinEvent(m_focusedHook);

        {
            var lockTaken = false;
            m_spinLock.TryEnter(ref lockTaken);

            if (!lockTaken) return;
        }

        m_focused.Dispose();
        m_spinLock.Exit();
    }
}
