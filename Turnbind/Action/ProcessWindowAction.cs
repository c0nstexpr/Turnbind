namespace Turnbind
{
    using Serilog;

    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Runtime.InteropServices;

    internal partial class ProcessWindowAction : IDisposable
    {
        static readonly ILogger _logger = Util.GetLogger<ProcessWindowAction>();

        public string? ProcessName { get; set; }

        public Process? Process
        {
            get
            {
                if (_process is not null) return _process;

                if (ProcessName is null) return null;

                var candidates = Process.GetProcessesByName(ProcessName);

                if (candidates.Length == 0)
                {
                    _logger.Warning("No processes found for {ProcessName}", ProcessName);
                    return null;
                }

                if (candidates.Length > 1)
                {
                    _logger.Warning("Multiple processes found for {ProcessName}", ProcessName);
                    return null;
                }

                _process = candidates[0];

                return _process;
            }

            private set => _process = value;
        }

        public IntPtr? WindowHandle => Process?.MainWindowHandle;


        readonly Subject<bool> _focused = new();

        public IObservable<bool> Focused => _focused;

        readonly IntPtr _focusedHook;

        readonly Subject<Unit> _destroyed = new();

        public IObservable<Unit> Destroyed => _destroyed;

        readonly IntPtr _destroyedHook;
        Process? _process = null;

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

            _focusedHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                (_, _, win, _, _, _, _) =>
                {
                    if (win == WindowHandle)
                    {
                        _logger.Information("Window focused");
                        _focused.OnNext(true);
                    }
                    else
                    {
                        _logger.Information("Window lost focuse");
                        _focused.OnNext(false);
                    }
                },
                0,
                0,
                WINEVENT_OUTOFCONTEXT
            );

            _destroyedHook = SetWinEventHook(
                EVENT_OBJECT_DESTROY,
                EVENT_OBJECT_DESTROY,
                IntPtr.Zero,
                (_, _, win, _, _, _, _) =>
                {
                    if (WindowHandle != win) return;

                    _logger.Information("Window destroyed");
                    _destroyed.OnNext(Unit.Default);

                    Process = null;
                },
                0,
                0,
                WINEVENT_OUTOFCONTEXT
            );
        }

        public void Dispose()
        {
            UnhookWinEvent(_focusedHook);
            UnhookWinEvent(_destroyedHook);
        }
    }
}
