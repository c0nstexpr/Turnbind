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
    readonly ILogger<ProcessWindowAction> m_log = App.GetRequiredService<ILogger<ProcessWindowAction>>();

    public string? ProcessName { get; set; }

    readonly Dictionary<HWND, Dictionary<int, Process>> m_processes = [];

    readonly BehaviorSubject<bool> m_focused = new(false);

    public BehaviorObservable<bool> Focused { get; }

    readonly UnhookWinEventSafeHandle m_focusedHook;
    readonly WINEVENTPROC m_focusedCallback;

    SpinLock m_spinLock = new();

    public ProcessWindowAction()
    {
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
        uint @event,
        HWND hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
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

    bool ContainsWin(HWND hwnd)
    {
        if (m_processes.ContainsKey(hwnd)) return true;

        foreach (var p in Process.GetProcessesByName(ProcessName))
        {
            HWND handle = new(p.MainWindowHandle);

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
        m_focusedHook.Dispose();

        {
            var lockTaken = false;
            m_spinLock.TryEnter(ref lockTaken);

            if (!lockTaken) return;
        }

        m_focused.Dispose();
        m_spinLock.Exit();
    }
}
