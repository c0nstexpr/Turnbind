using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive;
using Windows.Win32.UI.Input;
using System.Windows.Interop;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Drawing;
using LanguageExt.UnitsOfMeasure;

namespace Turnbind.Action;

sealed class TurnTickAction : IDisposable
{
    readonly ILogger<TurnTickAction> m_log = App.GetRequiredService<ILogger<TurnTickAction>>();

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

    #region Raw mouse input

    static readonly uint m_rawInputDeviceTypeSize = (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE));

    static readonly uint m_rawInputHeaderTypeSize = (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER));

    static readonly uint m_rawInputTypeSize = (uint)Marshal.SizeOf(typeof(RAWINPUT));

    long m_delta;

    int m_absX;

    static RAWINPUTDEVICE[] m_rawInputDevice => [
        new RAWINPUTDEVICE()
        {
            usUsagePage = PInvoke.HID_USAGE_PAGE_GENERIC,
            usUsage = PInvoke.HID_USAGE_GENERIC_MOUSE
        }
    ];

    HwndSource? m_winSrc;

    public HwndSource? WinSrc
    {
        get => m_winSrc;

        set
        {
            m_winSrc = value;

            var device = m_rawInputDevice;

            if (value is null)
            {
                device[0].dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_REMOVE;

                if (!PInvoke.RegisterRawInputDevices(device, m_rawInputDeviceTypeSize))
                {
                    m_log.LogError("Failed to unregister raw input device");
                    throw new InvalidOperationException();
                }

                return;
            }

            value.AddHook(WndProc);

            device[0].dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK;
            device[0].hwndTarget = new(value.Handle);

            if (!PInvoke.RegisterRawInputDevices(device, m_rawInputDeviceTypeSize))
            {
                m_log.LogError("Failed to register mouse raw input");
                throw new InvalidOperationException();
            }

        }
    }

    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == PInvoke.WM_INPUT) OnInput(lParam);
        return IntPtr.Zero;
    }

    unsafe void OnInput(nint lParam)
    {
        uint size = 0;
        var buffer = stackalloc RAWINPUT[1];

        PInvoke.GetRawInputData(
            new(lParam),
            RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT,
            null,
            ref size,
            m_rawInputHeaderTypeSize
        );

        var copiedSize = PInvoke.GetRawInputData(
            new(lParam),
            RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT,
            buffer,
            ref size,
            m_rawInputHeaderTypeSize
        );

        if (copiedSize == 0)
        {
            m_log.LogError("Failed to get mouse raw input data");
            return;
        }

        if (buffer->header.dwType != 0) return;

        var rawMouse = buffer->data.mouse;

        OnMouseInput(rawMouse.usFlags, rawMouse.lLastX);
    }

    void OnMouseInput(MOUSE_STATE flag, int x)
    {
        if ((flag & MOUSE_STATE.MOUSE_MOVE_ABSOLUTE) != default)
        {
            long left = 0;
            long right;
            // long top = 0;
            // long bottom;

            if ((flag & MOUSE_STATE.MOUSE_VIRTUAL_DESKTOP) == default)
            {
                right = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
                // bottom = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
            }
            else
            {
                left = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);
                right = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);
                // top = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);
                // bottom = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);
            }

            var newAbsX = (int)(x * right / ushort.MaxValue + left);

            x = newAbsX - m_absX;
            m_absX = newAbsX;
        }

        if (x == 0) return;

        Schedule(() => m_delta += x);
    }

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

    void Tick()
    {
        var dx = m_dirSpeed + m_remain + m_delta * MouseFactor;
        var input_dx = (int)dx;

        m_remain = dx - input_dx;

        if (input_dx == 0) return;

        m_mouseInput.dx = input_dx;

        if (PInvoke.SendInput(m_inputs, m_inputTypeSize) == 0)
            m_log.LogWarning("Mouse move input was blocked");
    }

    void Schedule(System.Action action) => m_scheduler.Schedule(action);

    public void Dispose()
    {
        WinSrc = null;
        m_tick.Dispose();
        m_scheduler.Dispose();
    }
}