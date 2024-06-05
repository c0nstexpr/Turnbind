using System.Reactive.Disposables;

using Microsoft.Extensions.Logging;

using Turnbind.Model;

namespace Turnbind.Action;

class BindControl : IDisposable
{
    readonly ILogger<BindControl> m_log = App.GetRequiredService<ILogger<BindControl>>();

    public required InputKeys Keys { get; init; }

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

    IDisposable? m_disposble;

    int m_instructionIndex = -1;

    public bool Enable
    {
        get => m_disposble is { };

        set
        {
            if (!value)
            {
                m_disposble?.Dispose();
                m_disposble = null;
                return;
            }

            if (m_disposble is { }) return;

            IDisposable? focusedDisposable = null;

            void OnFocuse(bool focused)
            {
                var turnAction = App.GetRequiredService<TurnAction>();
                var inputAction = App.GetRequiredService<InputAction>();

                void OnActive(bool active)
                {
                    m_log.LogInformation(
                        "{active} {keys} input for {dir} at {speed}p/ms with mouse move {factor}",
                        active ? "Activate" : "Deactivate",
                        string.Join(" + ", ((IEnumerable<InputKey>)Keys).Select(k => $"{k}")),
                        m_dir,
                        Setting.PixelPerMs,
                        Setting.MouseMoveFactor
                    );

                    if (!active)
                    {
                        if (m_instructionIndex != -1)
                        {
                            turnAction.UpdateDirection(m_instructionIndex, TurnInstruction.Stop);
                            m_instructionIndex = -1;
                        }
                        return;
                    }

                    m_instructionIndex = turnAction.InputDirection(m_dir);
                    turnAction.PixelPerMs = Setting.PixelPerMs;
                    turnAction.MouseFactor = Setting.MouseMoveFactor;
                }

                if (!focused)
                {
                    OnActive(false);
                    focusedDisposable?.Dispose();
                    focusedDisposable = null;
                    return;
                }

                if (focusedDisposable is { }) return;

                focusedDisposable = inputAction.SubscribeKeys(Keys).Subscribe(OnActive);
            }

            m_disposble = new CompositeDisposable()
            {
                Disposable.Create(() => OnFocuse(false)),
                App.GetRequiredService<ProcessWindowAction>().Focused.Subscribe(OnFocuse)
            };
        }
    }
    public void Dispose() => Enable = false;
}
