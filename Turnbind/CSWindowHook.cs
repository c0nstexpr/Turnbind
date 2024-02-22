namespace Turnbind
{
    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Runtime.InteropServices;

    internal partial class CSWindowHook : IDisposable
    {
        public Process Process { get; set; }

        public IntPtr WindowHandle => Process.MainWindowHandle;


        readonly Subject<Unit> _focused = new();

        public IObservable<Unit> Focused => _focused;

        readonly IntPtr _focusedHook;

        readonly Subject<Unit> _destroyed = new();

        public IObservable<Unit> Destroyed => _destroyed;

        readonly IntPtr _destroyedHook;

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

        public CSWindowHook(string processName)
        {
            const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
            const uint EVENT_OBJECT_DESTROY = 0x8001;
            const uint WINEVENT_OUTOFCONTEXT = 0x0000;

            Process = Process.GetProcessesByName(processName).Single();

            _focusedHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND,
                WindowHandle,
                (_, _, _, _, _, _, _) => _focused.OnNext(Unit.Default),
                0,
                0,
                WINEVENT_OUTOFCONTEXT
            );

            _destroyedHook = SetWinEventHook(
                EVENT_OBJECT_DESTROY,
                EVENT_OBJECT_DESTROY,
                WindowHandle,
                (_, _, _, _, _, _, _) => _destroyed.OnNext(Unit.Default),
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
