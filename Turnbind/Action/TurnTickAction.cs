using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly ILogger<TurnTickAction> m_log = App.GetRequiredService<ILogger<TurnTickAction>>();

    readonly InputAction m_inputAction = App.GetRequiredService<InputAction>();

    readonly EventLoopScheduler m_scheduler = new();

    readonly SerialDisposable m_tick = new();

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
                m_tick.Disposable = null;

                Schedule(
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

            m_tick.Disposable ??= m_scheduler.SchedulePeriodic<Unit>(default, Interval, _ => Tick());

            Schedule(
                () =>
                {
                    m_delta = 0;
                    m_dirSpeed = -m_dirSpeed;
                    m_instruction = value;
                    m_log.LogInformation("Set Instruction {i}", m_instruction);
                }
            );
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
                m_tick.Disposable = m_scheduler.SchedulePeriodic<Unit>(default, value, _ => Tick());
                m_log.LogInformation("Set timer interval {i} ms", m_interval.TotalMicroseconds);
            }
        }
    }

    public double MouseFactor { get; set; }

    double m_dirSpeed;

    double m_pixelSpeed;

    public double PixelSpeed
    {
        get => m_pixelSpeed;
        
        set
        {
            Schedule(() => m_dirSpeed = Instruction == TurnInstruction.Left ? value : -value);
            m_pixelSpeed = value;
        }
    }

    double m_remain;

    #region Listen the mouse move

    readonly IDisposable m_mouseMoveDisposable;

    long m_delta;

    #endregion

    public TurnTickAction()
    {
        var preX = m_inputAction.Point.X;

        m_mouseMoveDisposable = m_inputAction.MouseMove
            .Select(
                p =>
                {
                    var dx = p.X - preX;
                    preX = p.X;
                    return dx;
                }
            )
            .SubscribeOn(m_scheduler)
            .Where(_ => Instruction == TurnInstruction.Stop)
            .Subscribe(dx => m_delta += dx);
    }

    void Tick()
    {
        var target_speed = m_dirSpeed + m_remain;
        var diff = (target_speed - m_delta) * MouseFactor;

        target_speed += diff;

        var input_dx = (int)target_speed;

        m_remain = target_speed - input_dx;
        m_delta = 0;

        if (input_dx == 0) return;

        m_mouseInput.dx = input_dx;

        if (PInvoke.SendInput(m_inputs, m_inputTypeSize) == 0)
            m_log.LogWarning("Mouse move input was blocked");
    }

    void Schedule(System.Action action) => m_scheduler.Schedule(action);

    public void Dispose()
    {
        m_mouseMoveDisposable.Dispose();
        m_tick.Dispose();
        m_scheduler.Dispose();
    }
}