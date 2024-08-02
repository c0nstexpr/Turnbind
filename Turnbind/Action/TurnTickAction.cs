using Windows.Win32;
using Microsoft.Extensions.Logging;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Reactive.Concurrency;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly EventLoopScheduler m_scheduler = new();

    readonly SerialDisposable m_tick = new();

    TurnInstruction m_currentInstruction;

    #region Send Input

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

    #endregion

    public TurnInstruction Instruction
    {
        get => m_currentInstruction;

        set
        {
            if (m_currentInstruction == value) return;

            m_currentInstruction = value;

            if (value == TurnInstruction.Stop)
            {
                m_tick.Disposable = null;

                m_log.LogInformation("Stop ticking");

                return;
            }

            Schedule(
                () =>
                {
                    m_speed = value == TurnInstruction.Left ? -m_pixelSpeed : m_pixelSpeed;

                    m_delta = 0;

                    m_remain = 0;

                    m_log.LogInformation("Set Instruction {i}", value);

                    Tick(default);
                }
            );

            m_tick.Disposable = m_scheduler.SchedulePeriodic<Unit>(default, Interval, Tick);
        }
    }

    public TimeSpan Interval { get; set; }

    double m_currentWheelFactor;

    double m_wheelFactor;

    public double WheelFactor
    {
        get => m_currentWheelFactor;
        set
        {
            m_currentWheelFactor = value;
            Schedule(() => m_wheelFactor = value);
        }
    }

    double m_currentPixelSpeed;

    double m_pixelSpeed;

    public double PixelSpeed
    {
        get => m_currentPixelSpeed;

        set
        {
            m_currentPixelSpeed = value;
            Schedule(() => m_pixelSpeed = value);
        }
    }

    readonly IDisposable m_mouseWheelDisposable;

    readonly ILogger<TurnTickAction> m_log;

    public TurnTickAction(ILogger<TurnTickAction> log, InputAction inputAction)
    {
        m_log = log;

        m_mouseWheelDisposable = inputAction.MouseRaw
            .Where(e => e.Event == PInvoke.WM_MOUSEWHEEL && Instruction != TurnInstruction.Stop)
            .Select(e => (short)(e.Data.mouseData >> 16))
            .SubscribeOn(m_scheduler)
            .Subscribe(d => m_delta += d);
    }

    #region Tick

    double m_speed;

    long m_delta;

    double m_remain;

    Unit Tick(Unit u)
    {
        var x = m_speed + m_delta * m_wheelFactor + m_remain;
        var round_x = (int)Math.Round(x);

        m_mouseInput.dx = round_x;

        if (!MoveMouse())
        {
            m_log.LogWarning("Send mouse move failed, x:{x}", round_x);
            return u;
        }

        m_remain = x - round_x;

        return u;
    }

    bool MoveMouse() => PInvoke.SendInput(m_inputs, m_inputTypeSize) != 0;

    #endregion

    void Schedule(System.Action action) => m_scheduler.Schedule<Unit>(
        default,
        TimeSpan.Zero,
        (_, _) =>
        {
            action();
            return Disposable.Empty;
        }
    );

    public void Dispose()
    {
        m_tick.Dispose();
        m_mouseWheelDisposable.Dispose();
        m_scheduler.Dispose();
    }
}