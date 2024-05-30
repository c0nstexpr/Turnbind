
using Microsoft.Extensions.Logging;


namespace Turnbind.Action;

sealed partial class TurnAction : IDisposable
{
    readonly ILogger<TurnAction> m_log = App.GetRequiredService<ILogger<TurnAction>>();

    public TimeSpan Interval
    {
        get => m_timer.Period;

        set
        {
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
            m_pixelPerMs = value;
            m_log.LogInformation("Changed to {p} Pixels/Sec", PixelPerMs);
        }
    }

    readonly List<TurnInstruction> m_directionQueue = [];

    TurnInstruction m_direction;

    public TurnInstruction Direction
    {
        get => m_direction;

        private set
        {
            if (value == m_direction) return;

            m_direction = value;

            m_log.LogInformation("Turn action changed to {Dir}", value);
        }
    }


    public double MouseFactor { get; set; }

    public TurnAction() => Task.Run(Tick);

    public int InputDirection(TurnInstruction value)
    {
        if (value == TurnInstruction.Stop) OnStopPop();
        else
        {
            m_directionQueue.Add(value);
            Direction = value;
        }

        m_log.LogInformation("Turn action changed to {Dir}", Direction);

        return m_directionQueue.Count - 1;
    }

    public void UpdateDirection(int index, TurnInstruction value)
    {
        m_directionQueue[index] = value;
        if (index == m_directionQueue.Count - 1) OnStopPop();
    }

    void OnStopPop()
    {
        while (m_directionQueue.Count > 0 && m_directionQueue[^1] == TurnInstruction.Stop)
            m_directionQueue.RemoveAt(m_directionQueue.Count - 1);
        Direction = m_directionQueue.Count == 0 ? TurnInstruction.Stop : m_directionQueue[^1];
    }


    public void Dispose() => m_timer.Dispose();
}
