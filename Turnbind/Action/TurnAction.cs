using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Turnbind.Action;

sealed partial class TurnAction : IDisposable
{
    static readonly int m_inputTypeSize = Marshal.SizeOf(typeof(INPUT));

    readonly INPUT[] m_inputs = [
        new()
        {
            type = INPUT_TYPE.INPUT_MOUSE,
            Anonymous = new()
            {
                mi = new() { dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE }
            }
        }
    ];

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

    double m_remain;

    double m_anchorX;

    readonly List<TurnInstruction> m_directions = [];

    TurnInstruction m_direction;

    public TurnInstruction Direction
    {
        get
        {
            TurnInstruction dir;
            var token = false;

            try
            {
                m_lock.Enter(ref token);
                dir = m_direction;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (token) m_lock.Exit();
            }

            return dir;
        }

        private set
        {
            var token = false;

            if (value == m_direction) return;

            m_direction = value;
            try
            {
                m_lock.Enter(ref token);

                void Reset()
                {
                    m_remain = 0;
                    m_anchorX = m_inputAction.Point.X;
                }

                switch (value)
                {
                    case TurnInstruction.Left:
                        if (m_pixelPerPeriod > 0) m_pixelPerPeriod = -m_pixelPerPeriod;
                        Reset();
                        break;

                    case TurnInstruction.Right:
                        if (m_pixelPerPeriod < 0) m_pixelPerPeriod = -m_pixelPerPeriod;
                        Reset();
                        break;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (token) m_lock.Exit();
            }

            m_log.LogInformation("Turn action changed to {Dir}", value);
        }
    }

    readonly PeriodicTimer m_timer = new(TimeSpan.FromMilliseconds(1));

    ref MOUSEINPUT m_mouseInput => ref m_inputs[0].Anonymous.mi;

    public void InputDeltaX(int dx)
    {
        var token = false;

        try
        {
            m_lock.Enter(ref token);
            m_mouseInput.dx = dx;
            m_remain = m_pixelPerPeriod + m_remain - dx;
        }
        catch
        {
            throw;
        }
        finally
        {
            if (token) m_lock.Exit();
        }

        PInvoke.SendInput(m_inputs, m_inputTypeSize);
    }

    readonly InputAction m_inputAction = App.GetRequiredService<InputAction>();

    public double MouseFactor { get; set; }

    SpinLock m_lock;

    public TurnAction()
    {
        m_pixelPerPeriod = m_pixelPerMs * m_timer.Period.TotalSeconds;
        Task.Run(Tick);
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

    async void Tick()
    {
        while (await m_timer.WaitForNextTickAsync())
        {
            var token = false;

            try
            {
                m_lock.Enter(ref token);

                if (Direction == TurnInstruction.Stop)
                    SpinWait.SpinUntil(() => Direction != TurnInstruction.Stop);
                else InputDeltaX((int)(m_pixelPerPeriod + m_remain + MouseFactor * (m_inputAction.Point.X - m_anchorX)));
            }
            finally
            {
                if (token) m_lock.Exit();
            }
        }
    }

    public void Dispose() => m_timer.Dispose();
}
