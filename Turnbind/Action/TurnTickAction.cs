using Windows.Win32;
using Microsoft.Extensions.Logging;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using Turnbind.Helper;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly System.Reactive.Concurrency.EventLoopScheduler m_scheduler = new();

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

    readonly IDisposable m_mouseMoveDisposable;

    readonly ILogger<TurnTickAction> m_log;

    Point m_mousePos;

    public TurnTickAction(ILogger<TurnTickAction> log, InputAction inputAction)
    {
        m_log = log;

        m_mouseMoveDisposable = inputAction.MouseMoveRaw
            .Select(p => Tuple.Create(p, Instruction != TurnInstruction.Stop))
            .SubscribeOn(m_scheduler)
            .Select(
                 t =>
                 {
                     var (mouse, isTurning) = t;
                     var p = mouse.pt;
                     long delta = 0;

                     if (isTurning && !IsInjected(mouse.flags))
                         delta += p.X - m_mousePos.X;

                     m_mousePos = p;

                     return delta;
                 }
            )
            .Buffer(3)
            .Subscribe(
                d =>
                {
                    long plusSum = 0;
                    long minusSum = 0;

                    byte plusCount = 0;
                    byte minusCount = 0;

                    foreach (var i in d)
                        if (i > 0)
                        {
                            plusSum += i;
                            ++plusCount;
                        }
                        else
                        {
                            minusSum += i;
                            ++minusCount;
                        }

                    m_delta += plusCount >= minusCount ? plusSum : minusSum;
                }
            );
    }

    static bool IsInjected(uint flags)
    {
        const uint test = PInvoke.LLMHF_INJECTED | PInvoke.LLMHF_LOWER_IL_INJECTED;
        return (flags & test) != 0;
    }

    #region Tick

    double m_speed;

    long m_delta;

    double m_remain;

    Unit Tick(Unit u)
    {
        m_log.LogDebug("Current delta:{delta}", m_delta);

        var x = m_speed + m_delta * m_mouseFactor + m_remain;
        var round_x = (int)Math.Round(x);

        m_mouseInput.dx = round_x;

        if (!MoveMouse())
        {
            m_log.LogWarning("Send mouse move failed, x:{x}", round_x);
            return u;
        }

        m_log.LogDebug("Sended mouse move, x:{x}", round_x);
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
        m_mouseMoveDisposable.Dispose();
        m_scheduler.Dispose();
    }
}