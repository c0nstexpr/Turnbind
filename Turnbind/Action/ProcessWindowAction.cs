using System.Diagnostics;
using System.Reactive.Subjects;

using Microsoft.Extensions.Logging;

using Turnbind.Helper;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace Turnbind;

partial class ProcessWindowAction : IDisposable
{
    readonly ILogger<ProcessWindowAction> m_log;

    public string? ProcessName { get; set; }

    readonly Dictionary<HWND, int> m_processes = [];

    public HWND FocusedWinHandle { get; private set; }

    readonly BehaviorSubject<bool> m_focused = new(false);

    public BehaviorObservable<bool> Focused { get; }

    readonly UnhookWinEventSafeHandle m_focusedHook;
    readonly WINEVENTPROC m_focusedCallback;

    public ProcessWindowAction(ILogger<ProcessWindowAction> logger)
    {
        m_log = logger;
        Focused = m_focused.AsObservable();
        m_focusedCallback = OnFocused;

        m_focusedHook = PInvoke.SetWinEventHook(
            PInvoke.EVENT_SYSTEM_FOREGROUND,
            PInvoke.EVENT_SYSTEM_FOREGROUND,
            null,
            m_focusedCallback,
            0,
            0,
            0
        );
    }

    void OnFocused(
        HWINEVENTHOOK hWinEventHook,
        uint e,
        HWND hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        lock (m_focused)
        {
            if (m_focused.IsDisposed) return;

            var focused = ContainsWin(hwnd);

            if (m_focused.Value == focused) return;

            if (focused)
            {
                FocusedWinHandle = hwnd;
                m_log.LogInformation("Window focused");
            }
            else
            {
                FocusedWinHandle = default;
                m_log.LogInformation("Window lost focuse");
            }

            m_focused.OnNext(focused);
        }
    }

    bool ContainsWin(HWND hwnd)
    {
        if (m_processes.ContainsKey(hwnd)) return true;

        foreach (var p in Process.GetProcessesByName(ProcessName))
        {
            HWND handle = new(p.MainWindowHandle);

            if (handle == default || m_processes.ContainsKey(handle)) continue;

            m_processes.Add(handle, p.Id);

            EventHandler onExit = default!;

            onExit = (s, e) =>
            {
                m_processes.Remove(handle);
                p.Exited -= onExit;
            };

            p.Exited += onExit;
        }

        return m_processes.ContainsKey(hwnd);
    }

    public void Dispose()
    {
        m_focusedHook.Dispose();

        lock (m_focused) m_focused.Dispose();
    }
}
