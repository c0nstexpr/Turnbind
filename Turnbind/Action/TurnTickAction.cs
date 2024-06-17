using Windows.Win32;
using Microsoft.Extensions.Logging;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly System.Reactive.Concurrency.EventLoopScheduler m_scheduler = new();

    readonly SerialDisposable m_tick = new();

    TurnInstruction m_currentInstruction;

    TurnInstruction m_instruction = TurnInstruction.Stop;

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

                Schedule(
                     () =>
                     {
                         m_delta = 0;
                         m_instruction = TurnInstruction.Stop;
                     }
                 );

                m_log.LogInformation("Stop ticking");
                return;
            }

            Schedule(
                () =>
                {
                    m_remain = 0;
                    m_delta = 0;
                    m_speed = value == TurnInstruction.Left ? -m_pixelSpeed : m_pixelSpeed;
                    m_instruction = value;
                    m_log.LogInformation("Set Instruction {i}", m_instruction);
                    Tick(default);
                }
            );

            m_tick.Disposable = m_scheduler.SchedulePeriodic<Unit>(default, Interval, Tick);
        }
    }

    public TimeSpan Interval { get; set; }

    double m_currentMouseFactor;

    double m_mouseFactor;

    public double MouseFactor
    {
        get => m_currentMouseFactor;
        set
        {
            m_currentMouseFactor = value;
            Schedule(() => m_mouseFactor = value);
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

    Point m_startPosition;

    long m_delta;

    Point m_mousePos;

    double m_speed;

    double m_remain;

    readonly IDisposable m_mouseMoveDisposable;

    readonly ILogger<TurnTickAction> m_log;

    readonly ProcessWindowAction m_windowAction;

    public TurnTickAction(ILogger<TurnTickAction> log, InputAction inputAction, ProcessWindowAction windowAction)
    {
        m_log = log;
        m_windowAction = windowAction;

        m_mouseMoveDisposable = inputAction.MouseMove.SubscribeOn(m_scheduler)
            .Subscribe(
                p =>
                {
                    if (Instruction != TurnInstruction.Stop && p != m_startPosition)
                        m_delta += p.X - m_mousePos.X;
                    m_mousePos = p;
                }
            );
    }

    Unit Tick(Unit u)
    {
        m_startPosition = m_mousePos;

        if (m_delta == 1) m_delta = 0; // ignore minor error

        var x = Math.Clamp(m_mousePos.X + m_speed + m_delta * m_mouseFactor + m_remain, 0, int.MaxValue);
        var input_x = (int)Math.Round(x);

        m_log.LogDebug("Current delta:{delta}", m_delta);

        if (input_x == 0)
        {
            m_remain = 0;
            return u;
        }

        m_remain = x - input_x;
        m_delta -= input_x;


        // https://stackoverflow.com/questions/4540282
        if (PInvoke.SendInput(input_x, m_mousePos.Y) > 0)
        {
            m_log.LogDebug("Sended mouse move, x:{x}", x);
            return u;
        }

        m_log.LogWarning("Send mouse move failed, x:{x}", x);

        return u;
    }

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
        m_mouseMoveDisposable.Dispose();
        m_scheduler.Dispose();
    }
}