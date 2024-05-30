using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Runtime.InteropServices;

namespace Turnbind.Action;
class TurnTickAction : IDisposable
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

    readonly PeriodicTimer m_timer = new(TimeSpan.FromMilliseconds(1));

    ref MOUSEINPUT m_mouseInput => ref m_inputs[0].Anonymous.mi;

    readonly InputAction m_inputAction = App.GetRequiredService<InputAction>();

    async void Tick()
    {
        double remain;
        double anchorX;

        while (await m_timer.WaitForNextTickAsync())
        {
            var token = false;

            if (value == TurnInstruction.Stop)
            {
                remain = 0;
                anchorX = m_inputAction.Point.X;
            }

            if (Direction == TurnInstruction.Stop)
                SpinWait.SpinUntil(() => Direction != TurnInstruction.Stop);
            else
            {

                var m_pixelPerPeriod = m_pixelPerMs * m_timer.Period.TotalSeconds;

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


                var dx = (int)(m_pixelPerPeriod + remain + MouseFactor * (m_inputAction.Point.X - anchorX));

                {
                    var token = false;

                    try
                    {
                        m_lock.Enter(ref token);
                        m_mouseInput.dx = dx;
                        remain = m_pixelPerPeriod + remain - dx;
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
            }
        }
    }

    public void Dispose() => m_timer.Dispose();
}
