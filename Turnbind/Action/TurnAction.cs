using Microsoft.Extensions.Logging;

namespace Turnbind.Action;

sealed partial class TurnAction : IDisposable
{
    readonly ILogger<TurnAction> m_log = App.GetRequiredService<ILogger<TurnAction>>();

    readonly TurnTickAction m_action = new();

    public TimeSpan Interval
    {
        get => m_action.Interval;

        set
        {
            m_action.Interval = value;
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
            m_action.PixelSpeed = value * Interval.TotalMilliseconds;
            m_log.LogInformation("Changed to {p} Pixels/Sec", PixelPerMs);
        }
    }

    readonly List<TurnInstruction> m_directionQueue = [];

    public TurnInstruction Direction
    {
        get => m_action.Instruction;

        private set
        {
            m_action.Instruction = value;
            m_log.LogInformation("Turn action changed to {Dir}", value);
        }
    }

    public double MouseFactor
    {
        get => m_action.MouseFactor;
        set
        {
            m_action.MouseFactor = value;
            m_log.LogInformation("Mouse factor chaged to {factor}", value);
        }
    }

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

    public void Dispose() => m_action.Dispose();
}
