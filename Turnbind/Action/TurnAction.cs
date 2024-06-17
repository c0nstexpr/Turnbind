using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

namespace Turnbind.Action;

sealed partial class TurnAction(ILogger<TurnAction> log, TurnTickAction action) :
    ObservableObject, IDisposable
{
    public TimeSpan Interval
    {
        get => action.Interval;

        set
        {
            action.Interval = value;
            action.PixelSpeed = PixelPerMs * Interval.TotalMilliseconds;
            OnPropertyChanged();
            log.LogInformation("Set Interval {interval} ms", value.TotalMilliseconds);
        }
    }

    double m_pixelPerMs = 1;

    public double PixelPerMs
    {
        get => m_pixelPerMs;

        set
        {
            m_pixelPerMs = value;
            action.PixelSpeed = PixelPerMs * Interval.TotalMilliseconds;
            OnPropertyChanged();
            log.LogInformation("Set PixelPerMs {p}", PixelPerMs);
        }
    }

    readonly List<TurnInstruction> m_directionQueue = [];

    public TurnInstruction Direction
    {
        get => action.Instruction;

        private set
        {
            action.Instruction = value;
            OnPropertyChanged();
            log.LogInformation("Set Direction {Dir}", value);
        }
    }

    public double MouseFactor
    {
        get => action.MouseFactor;
        set
        {
            action.MouseFactor = value;
            OnPropertyChanged();
            log.LogInformation("Set MouseFactor {factor}", value);
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

    public void Dispose() => action.Dispose();
}
