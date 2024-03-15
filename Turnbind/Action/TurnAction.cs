using Serilog;

using SharpHook;

using Turnbind.Model;

namespace Turnbind.Action;

sealed class TurnAction : IDisposable
{
    readonly ILogger m_log = Log.ForContext<TurnAction>();


    public readonly EventSimulator Simulator = new();

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
        while (true)
        {
            if (Direction != TurnInstruction.Stop)
            {
                m_log.Information("Simulate turn {Direction}", Direction.ToString());

                Simulator.Turn(
                    Direction == TurnInstruction.Left ? TurnDirection.Left : TurnDirection.Right,
                    PixelPerSec * m_timer.Period.TotalSeconds
                );
            }
            else m_log.Information("Stop simulate turning");

            if (!await m_timer.WaitForNextTickAsync()) break;
        }
    }

    public void Dispose() => m_timer.Dispose();
}
