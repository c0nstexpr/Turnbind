using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly ILogger<TurnTickAction> m_log = App.GetRequiredService<ILogger<TurnTickAction>>();

    #region Win32 Input

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

    ref MOUSEINPUT m_mouseInput => ref m_inputs[0].Anonymous.mi;

    readonly InputAction m_inputAction = App.GetRequiredService<InputAction>();

    #endregion

    TurnInstruction m_instruction = TurnInstruction.Stop;

    public TurnInstruction Instruction
    {
        get => m_instruction;
        set
        {
            if (m_instruction == value) return;

            if (value == TurnInstruction.Stop)
            {

                m_timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
                m_remain = 0;
                m_instruction = TurnInstruction.Stop;

                m_log.LogInformation("Stop ticking");
                return;
            }

            m_timer.Change(TimeSpan.Zero, Interval);
            m_anchorX = m_inputAction.Point.X;
            GetSpeed();

            m_instruction = value;

            m_log.LogInformation("Set instruction {i}", m_instruction);
        }
    }

    TimeSpan m_interval;

    public TimeSpan Interval
    {
        get => m_interval;
        set
        {
            m_interval = value;

            m_log.LogInformation("Set interval {i} ms", m_interval.TotalMicroseconds);

            if (m_instruction != TurnInstruction.Stop)
            { 
                m_timer.Change(TimeSpan.Zero, value);
                m_log.LogInformation("Set timer interval {i} ms", m_interval.TotalMicroseconds);
            }
        }
    }

    public double MouseFactor { get; set; }

    double m_speed;

    double m_pixelSpeed;

    public double PixelSpeed
    {
        get => m_pixelSpeed;
        set
        {
            m_pixelSpeed = value;
            GetSpeed();
        }
    }

    double m_remain;

    double m_anchorX;

    readonly Timer m_timer;

    public TurnTickAction() => m_timer = new Timer(TurnTick, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

    void GetSpeed() => m_speed = Instruction == TurnInstruction.Left ? PixelSpeed : -PixelSpeed;

    void TurnTick(object? state)
    {
        var dx = m_remain + m_speed + MouseFactor * (m_inputAction.Point.X - m_anchorX);
        var input_dx = (int)dx;

        m_mouseInput.dx = input_dx;
        m_anchorX += input_dx;
        m_remain = dx - input_dx;

        if (PInvoke.SendInput(m_inputs, m_inputTypeSize) == 0)
            m_log.LogWarning("Mouse move input was blocked");
    }

    public void Dispose() => m_timer.Dispose();
}