using System.Runtime.InteropServices;

using Serilog;

using Turnbind.Helper;

namespace Turnbind.Action;

sealed partial class TurnAction : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MousePoint(int x, int y)
    {
        public int X = x;
        public int Y = y;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int x, int y);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out MousePoint lpPoint);

    readonly ILogger m_log = Log.ForContext<TurnAction>();

    public TimeSpan Interval
    {
        get => m_timer.Period;

        set => m_timer.Period = value;
    }

    public double PixelPerSec { get; set; }

    public TurnInstruction Direction { get; set; }

    readonly PeriodicTimer m_timer = new(TimeSpan.FromSeconds(1) / 288);

    public TurnAction() => Tick();

    async void Tick()
    {
        var remain = 0.0;
        var isRunning = false;
        var preDirection = TurnInstruction.Stop;

        while (true)
        {
            if (Direction != TurnInstruction.Stop)
            {
                var p = PixelPerSec * m_timer.Period.TotalSeconds;

                if (Direction == TurnInstruction.Left) p = -p;

                p += remain;

                var intP = (int)Math.Clamp(p, int.MinValue, int.MaxValue);

                GetCursorPos(out var pos);
                SetCursorPos(pos.X + intP, pos.Y);
                remain = p - intP;

                isRunning = true;

                if (preDirection != Direction)
                {
                    m_log.WithSourceInfo().Information("Turn action changed, {Dir} direction, {p} Pixels/Sec", Direction, PixelPerSec);
                    preDirection = Direction;
                }
            }
            else if (isRunning)
            {
                m_log.WithSourceInfo().Information("Turn action Stop");
                preDirection = TurnInstruction.Stop;

                isRunning = false;
                remain = 0;
            }

            if (!await m_timer.WaitForNextTickAsync()) break;
        }
    }

    public void Dispose() => m_timer.Dispose();
}
