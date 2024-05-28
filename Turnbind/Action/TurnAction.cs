using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

namespace Turnbind.Action;

sealed partial class TurnAction : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
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

    readonly ILogger<TurnAction> m_log = App.GetRequiredService<ILogger<TurnAction>>();

    public TimeSpan Interval
    {
        get => m_timer.Period;

        set
        {
            m_pixelPerPeriod = m_pixelPerPeriod * (value / m_timer.Period);
            m_timer.Period = value;
            m_log.LogInformation("Turn Interval changed to {ms} ms", value.Milliseconds);
        }
    }

    double m_pixelPerMs = 1;

    public double PixelPerMs
    {
        get => m_pixelPerMs;

        set
        {
            m_pixelPerPeriod = m_pixelPerPeriod / m_pixelPerMs * value;
            m_pixelPerMs = value;
            m_log.LogInformation("Changed to {p} Pixels/Sec", PixelPerMs);
        }
    }

    double m_pixelPerPeriod;

    readonly List<TurnInstruction> m_directions = [];

    readonly IDisposable m_mouseMoveDisposable;

    TurnInstruction m_direction;

    public TurnInstruction Direction
    {
        get => m_direction;
        
        private set
        {
            m_direction = value;

            switch (value)
            {
                case TurnInstruction.Left:
                    if (m_pixelPerPeriod > 0) m_pixelPerPeriod = -m_pixelPerPeriod;
                    break;

                case TurnInstruction.Right:
                    if (m_pixelPerPeriod < 0) m_pixelPerPeriod = -m_pixelPerPeriod;
                    break;
            }

            m_log.LogInformation("Turn action changed to {Dir}", value);
        }
    }

    public int InputDirection(TurnInstruction value)
    {
        if (value == TurnInstruction.Stop) OnStopPop();
        else
        {
            m_directions.Add(value);
            Direction = value;
        }

        m_log.LogInformation("Turn action changed to {Dir}", Direction);

        return m_directions.Count - 1;
    }

    public void UpdateDirection(int index, TurnInstruction value)
    {
        m_directions[index] = value;
        if (index == m_directions.Count - 1) OnStopPop();
    }

    void OnStopPop()
    {
        while (m_directions.Count > 0 && m_directions[^1] == TurnInstruction.Stop)
            m_directions.RemoveAt(m_directions.Count - 1);
        Direction = m_directions.Count == 0 ? TurnInstruction.Stop : m_directions[^1];
    }

    readonly PeriodicTimer m_timer = new(TimeSpan.FromMilliseconds(1));

    public TurnAction()
    {
        var inputAction = App.GetRequiredService<InputAction>();

        m_pixelPerPeriod = m_pixelPerMs * m_timer.Period.TotalSeconds;
        m_mouseMoveDisposable = inputAction.MouseMove.Subscribe(
            point =>
            {
            }
        );

        Task.Run(Tick);
    }

    async void Tick()
    {
        Input[] inputs = [new() { mi = new() { dwFlags = 0x0001 } }];
        var remain = 0.0;

        while (await m_timer.WaitForNextTickAsync())
            if (Direction == TurnInstruction.Stop)
            {
                remain = 0;
                SpinWait.SpinUntil(() => Direction != TurnInstruction.Stop);
            }
            else
            {
                var intP = (int)(m_pixelPerPeriod + remain);

                remain = m_pixelPerPeriod + remain - intP;

                inputs[0].mi.dx = intP;

                SendInput(1, inputs, Input.Size);
            }
    }

    public void Dispose() => m_timer.Dispose();
}
