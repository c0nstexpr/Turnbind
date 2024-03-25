using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

namespace Turnbind.Action;

sealed partial class TurnAction : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    private struct Input
    {
        public uint type;
        public MouseInput mi;

        public static readonly int Size = Marshal.SizeOf(typeof(Input));
    }

    [LibraryImport("user32", SetLastError = true)]
    private static partial uint SendInput(uint inputs, [In] Input[] input, int size);

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetPriorityClass(nint hProcess, uint dwPriorityClass);

    readonly ILogger<TurnAction> m_log = App.GetService<ILogger<TurnAction>>();

    public TimeSpan Interval
    {
        get => m_timer.Period;

        set => m_timer.Period = value;
    }

    public double PixelPerSec { get; set; }

    public TurnInstruction Direction { get; set; }

    readonly PeriodicTimer m_timer = new(TimeSpan.FromSeconds(1) / 288);

    public TurnAction()
    {
        Tick();
        SetPriorityClass(Process.GetCurrentProcess().Handle, 0x00000080);
    }

    async void Tick()
    {
        var remain = 0.0;
        var isRunning = false;
        var preDirection = TurnInstruction.Stop;
        var inputs = new Input[1];

        inputs[0] = new() { mi = new() { dwFlags = 0x0001 } };

        while (true)
        {
            if (Direction != TurnInstruction.Stop)
            {
                var p = PixelPerSec * m_timer.Period.TotalSeconds;

                if (Direction == TurnInstruction.Left) p = -p;

                p += remain;

                var intP = (int)Math.Clamp(p, int.MinValue, int.MaxValue);

                inputs[0].mi.dx = intP;

                SendInput(1, inputs, Input.Size);

                remain = p - intP;

                isRunning = true;

                if (preDirection != Direction)
                {
                    m_log.LogInformation("Turn action changed, {Dir} direction, {p} Pixels/Sec", Direction, PixelPerSec);
                    preDirection = Direction;
                }
            }
            else if (isRunning)
            {
                m_log.LogInformation("Turn action Stop");
                preDirection = TurnInstruction.Stop;

                isRunning = false;
                remain = 0;
            }

            if (!await m_timer.WaitForNextTickAsync()) break;
        }
    }

    public void Dispose() => m_timer.Dispose();
}
