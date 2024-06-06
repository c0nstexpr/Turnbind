using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly ILogger<TurnTickAction> m_log = App.GetRequiredService<ILogger<TurnTickAction>>();

    readonly InputAction m_inputAction = App.GetRequiredService<InputAction>();

    SpinLock m_lock;

    #region Win32 Input

    static readonly int m_inputTypeSize = Marshal.SizeOf(typeof(INPUT));

    readonly INPUT[] m_inputs = [
        new()
        {
            type = INPUT_TYPE.INPUT_MOUSE,
            Anonymous = new()
            {
                mi = new() 
                { 
                    dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE
                }
            }
        }
    ];

    ref MOUSEINPUT m_mouseInput => ref m_inputs[0].Anonymous.mi;

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

                Lock(
                    () =>
                    {
                        m_delta = 0;
                        m_remain = 0;
                        m_instruction = TurnInstruction.Stop;
                    }
                );

                m_log.LogInformation("Stop ticking");
                return;
            }

            m_timer.Change(TimeSpan.Zero, Interval);

            Lock(
                () =>
                {
                    m_delta = 0;
                    GetSpeed();
                }
            );

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
            Lock(GetSpeed);
        }
    }

    double m_remain;

    readonly Timer m_timer;

    #region Listen the mouse move

    readonly IDisposable m_mouseMoveDisposable;

    readonly List<int> m_filtered = [];

    double m_delta;

    int m_preX;

    #endregion

    public TurnTickAction()
    {
        m_timer = new Timer(_ => Lock(() => Input(m_speed + m_delta)), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

        PInvoke.SetMessageExtraInfo(m_mouseInput.dwExtraInfo);

        m_preX = m_inputAction.Point.X;
        m_mouseMoveDisposable = m_inputAction.MouseMove
            .Select(
            p =>
                {
                    var dx = p.X - m_preX;
                    m_preX = p.X;
                    return dx;
                }
            )
            .Where(
                dx =>
                {
                    if (Instruction == TurnInstruction.Stop || dx == 0) return false;

                    var accepted = true;

                    Lock(
                        () =>
                        {
                            var i = m_filtered.BinarySearch(dx);

                            if (i < 0) return;

                            m_log.LogInformation("Filter move event");
                            m_filtered.RemoveAt(i);

                            accepted = false;
                        }
                    );

                    return accepted;
                }
            )
            .Subscribe(x => Lock(() => m_delta += MouseFactor * x));
    }

    void Lock(System.Action action)
    {
        if (m_lock.IsHeldByCurrentThread)
        {
            action();
            return;
        }

        var token = false;

        try
        {
            m_lock.Enter(ref token);
            action();
        }
        catch (Exception e)
        {
            m_log.LogError("Failed to enter lock: {e}", e);
            return;
        }
        finally
        {
            if (token) m_lock.Exit();
        }
    }

    void GetSpeed() => m_speed = Instruction == TurnInstruction.Left ? PixelSpeed : -PixelSpeed;

    void Input(double dx)
    {
        dx += m_remain;

        var input_dx = (int)dx;

        m_remain = dx - input_dx;

        m_mouseInput.dx = input_dx;

        if (input_dx == 0) return;

        if (PInvoke.SendInput(m_inputs, m_inputTypeSize) == 0)
            m_log.LogWarning("Mouse move input was blocked");
        else
        {
            var i = m_filtered.BinarySearch(input_dx);
            m_filtered.Insert(i > 0 ? i : ~i, input_dx);
        }
    }

    public void Dispose()
    {
        m_mouseMoveDisposable.Dispose();
        m_timer.Dispose();
    }
}