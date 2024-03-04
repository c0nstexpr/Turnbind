using SharpHook;

using Turnbind.Model;

namespace Turnbind.Action;

public class TurnAction : IDisposable
{
    public enum Instruction
    {
        Stop,
        Left,
        Right
    }

    public readonly EventSimulator Simulator = new();

    public double Interval
    {
        get => 1 / m_timer.Period.TotalSeconds;

        set => m_timer.Period = TimeSpan.FromSeconds(1) / value;
    }

    public double PixelPerSec { get; set; }

    public Instruction Direction { get; set; }

    readonly PeriodicTimer m_timer = new(TimeSpan.FromSeconds(1) / 288);

    public TurnAction() => Tick();

    async void Tick()
    {
        while (true)
        {            
            if(Direction != Instruction.Stop)
                Simulator.Turn(
                    Direction == Instruction.Left ? TurnDirection.Left : TurnDirection.Right,
                    PixelPerSec * m_timer.Period.TotalSeconds
                );

            if (!await m_timer.WaitForNextTickAsync()) break;
        }
    }

    public void Dispose() => m_timer.Dispose();
}
