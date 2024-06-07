using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

namespace Turnbind.Action;

sealed partial class TurnAction : ObservableObject, IDisposable
{
    readonly ILogger<TurnAction> m_log = App.GetRequiredService<ILogger<TurnAction>>();

    readonly TurnTickAction m_action = new();

    TimeSpan m_interval;

    public TimeSpan Interval
    {
        get => m_interval;

        set
        {
            m_interval = value;
            m_action.Interval = value;
            OnPropertyChanged();
            m_log.LogInformation("Set Interval {interval} ms", value.TotalMilliseconds);
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
            OnPropertyChanged();
            m_log.LogInformation("Set PixelPerMs {p}", PixelPerMs);
        }
    }

    readonly List<TurnInstruction> m_directionQueue = [];

    TurnInstruction m_dir;

    public TurnInstruction Direction
    {
        get => m_dir;

        private set
        {
            m_dir = value;
            m_action.Instruction = value;
            OnPropertyChanged();
            m_log.LogInformation("Set Direction {Dir}", value);
        }
    }

    double m_factor;

    public double MouseFactor
    {
        get => m_factor;
        set
        {
            m_factor = value;
            m_action.MouseFactor = value;
            OnPropertyChanged();
            m_log.LogInformation("Set MouseFactor {factor}", value);
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

    public void Dispose() => m_action.Dispose();
}
