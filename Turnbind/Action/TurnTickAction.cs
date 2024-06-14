using Windows.Win32;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Reactive.Linq;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly EventLoopScheduler m_scheduler = new();

    readonly SerialDisposable m_tick = new();

    #region Send Input

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

            Schedule(
                () =>
                {
                    m_delta = 0;
                    m_remain = 0;
                    m_dirSpeed = value == TurnInstruction.Left ? -PixelSpeed : PixelSpeed;
                    m_instruction = value;
                    m_log.LogInformation("Set Instruction {i}", m_instruction);
                    Tick();
                }
            );

            m_tick.Disposable ??= m_scheduler.SchedulePeriodic<Unit>(default, Interval, _ => Tick());
        }
    }

    TimeSpan m_interval;

    public TimeSpan Interval
    {
        get => m_interval;
        set
        {
            m_interval = value;

            m_log.LogInformation("Set interval {i} ms", m_interval.TotalMilliseconds);

            if (m_instruction != TurnInstruction.Stop)
                m_tick.Disposable = m_scheduler.SchedulePeriodic<Unit>(default, value, _ => Tick());
        }
    }

    public double MouseFactor { get; set; }

    double m_dirSpeed;

    public double PixelSpeed { get; set; }

    double m_remain;

    long m_delta;

    int m_preX;

    readonly IDisposable m_mouseMoveDisposable;

    readonly ILogger<TurnTickAction> m_log;

    public TurnTickAction(ILogger<TurnTickAction> log, InputAction inputAction)
    {
        m_log = log;

        m_mouseMoveDisposable = inputAction.MouseMove.Select(p => p.X)
            .SubscribeOn(m_scheduler)
            .Subscribe(
                x =>
                {
                    if (Instruction != TurnInstruction.Stop) m_delta += x - m_preX;
                    m_preX = x;
                }
            );
    }

    void Tick()
    {
        var x = m_dirSpeed + m_remain + m_delta * MouseFactor;
        var input_x = (int)Math.Clamp(x, int.MinValue, int.MaxValue);

        m_remain = x - input_x;

        if (input_x == 0) return;

        m_mouseInput.dx = input_x;
        m_delta -= input_x;

        if (PInvoke.SendInput(m_inputs, m_inputTypeSize) > 0)
        {
            m_log.LogInformation("Sended mouse move x:{x}", x);
            return;
        }

        m_log.LogWarning("Send mouse move input blocked");
    }

    void Schedule(System.Action action) => m_scheduler.Schedule(action);

    public void Dispose()
    {
        m_tick.Dispose();
        m_scheduler.Dispose();
    }
}