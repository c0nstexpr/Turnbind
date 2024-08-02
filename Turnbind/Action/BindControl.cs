using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Microsoft.Extensions.Logging;

using Turnbind.Model;

namespace Turnbind.Action;

class BindControl : IDisposable
{
    readonly ILogger<BindControl> m_log = App.GetRequiredService<ILogger<BindControl>>();

    readonly TurnAction m_turnAction = App.GetRequiredService<TurnAction>();

    readonly InputAction m_inputAction = App.GetRequiredService<InputAction>();

    readonly CompositeDisposable m_disposble;

    readonly InputKeys m_keys;

    readonly string m_keysStr;

    public required InputKeys Keys
    {
        get => m_keys;

        [MemberNotNull(nameof(m_keys), nameof(m_disposble), nameof(m_keysStr))]
        init
        {
            m_keys = value;
            m_disposble = [
                App.GetRequiredService<ProcessWindowAction>().Focused.Subscribe(OnFocused),
                m_inputAction.SubscribeKeys(m_keys).Subscribe(OnActive)
            ];
            m_keysStr = string.Join(" + ", ((IEnumerable<InputKey>)Keys).Select(k => $"{k}"));
        }
    }

    TurnSetting m_setting = new();

    public required TurnSetting Setting
    {
        get => m_setting;

        set
        {
            m_setting = value;
            m_dir = value.Dir switch
            {
                TurnDirection.Left => TurnInstruction.Left,
                TurnDirection.Right => TurnInstruction.Right,
                _ => TurnInstruction.Stop
            };

            if (Enable)
            {
                Enable = false;
                Enable = true;
            }
        }
    }

    TurnInstruction m_dir;

    int m_instructionIndex = -1;

    bool m_focused;

    bool m_enable;

    public bool Enable
    {
        get => m_enable;

        set
        {
            if (!value) Deactivate();

            m_enable = value;
        }
    }

    void OnFocused(bool focused)
    {
        if (!focused) Deactivate();

        m_focused = focused;
    }

    void OnActive(bool active)
    {
        if (!(m_focused && Enable)) return;

        m_log.LogInformation(
            "{active} {keys} input for {dir} at {speed}p/ms with mouse move {factor}",
            active ? "Activate" : "Deactivate",
            m_keysStr,
            m_dir,
            Setting.PixelPerMs,
            Setting.WheelFactor
        );

        if (active) Activate();
        else Deactivate();
    }

    void Deactivate()
    {
        if (m_instructionIndex == -1) return;

        m_turnAction.UpdateDirection(m_instructionIndex, TurnInstruction.Stop);
        m_instructionIndex = -1;
    }

    void Activate()
    {
        m_turnAction.PixelPerMs = Setting.PixelPerMs;
        m_turnAction.WheelFactor = Setting.WheelFactor;
        m_instructionIndex = m_turnAction.InputDirection(m_dir);
    }

    public void Dispose()
    {
        Deactivate();
        m_disposble.Dispose();
    }
}
