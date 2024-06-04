using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Runtime.InteropServices;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
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

    TurnInstruction m_instruction;

    public TurnInstruction Instruction
    {
        get => m_instruction;
        set
        {
            if (m_instruction == value) return;

            if (m_instruction == TurnInstruction.Stop)
            {
                m_timer.Change(default, Timeout.InfiniteTimeSpan);
                return;
            }

            if (m_instruction == TurnInstruction.Stop)
                m_timer.Change(default, m_interval);

            m_remain = 0;
            m_anchorX = m_inputAction.Point.X;
            GetSpeed();

            m_instruction = value;
        }
    }

    TimeSpan m_interval;

    public TimeSpan Interval
    {
        get => m_interval;
        set
        {
            m_interval = value;
            m_timer.Change(default, value);
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

    public TurnTickAction() => m_timer = new Timer(TurnTick);

    void GetSpeed() => m_speed = Instruction == TurnInstruction.Left ? PixelSpeed : -PixelSpeed;

    void TurnTick(object? state)
    {
        var dx = m_remain + m_speed + MouseFactor * (m_inputAction.Point.X - m_anchorX);

        m_mouseInput.dx = (int)dx;

        m_remain = dx - m_mouseInput.dx;

        PInvoke.SendInput(m_inputs, m_inputTypeSize);
    }

    public void Dispose() => m_timer.Dispose();
}