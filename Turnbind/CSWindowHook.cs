namespace Turnbind
{
    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Runtime.InteropServices;

    internal class CSWindowHook : IDisposable
    {
        public Process Process { get; set; }

        public IntPtr WindowHandle => Process.MainWindowHandle;


        readonly Subject<Unit> focused = new();

        public IObservable<Unit> Focused => focused;

        readonly IntPtr focusedHook;

        readonly Subject<Unit> destroyed = new();

        public IObservable<Unit> Destroyed => destroyed;

        readonly IntPtr destroyedHook;

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        public CSWindowHook(string processName)
        {
            const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
            const uint EVENT_OBJECT_DESTROY = 0x8001;
            const uint WINEVENT_OUTOFCONTEXT = 0x0000;

            Process = Process.GetProcessesByName(processName).Single();

            focusedHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND,
                WindowHandle,
                (_, _, _, _, _, _, _) => focused.OnNext(Unit.Default),
                0,
                0,
                WINEVENT_OUTOFCONTEXT
            );

            destroyedHook = SetWinEventHook(
                EVENT_OBJECT_DESTROY,
                EVENT_OBJECT_DESTROY,
                WindowHandle,
                (_, _, _, _, _, _, _) => destroyed.OnNext(Unit.Default),
                0,
                0,
                WINEVENT_OUTOFCONTEXT
            );
        }

        public void Dispose()
        {
            UnhookWinEvent(focusedHook);
            UnhookWinEvent(destroyedHook);
        }
    }
}
