using System.Reactive.Disposables;

using Turnbind.Model;

namespace Turnbind.Action;

class BindControl : IDisposable
{
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
        }
    }

    TurnInstruction m_dir;

    IDisposable? m_disposble;

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
                var index = -1;
                var turnAction = App.GetRequiredService<TurnAction>();
                var inputAction = App.GetRequiredService<InputAction>();

                void OnActive(bool active)
                {

                    if (!active)
                    {
                        if (index != -1) turnAction.UpdateDirection(index, TurnInstruction.Stop);
                        return;
                    }

                    index = turnAction.InputDirection(m_dir);
                    turnAction.PixelPerMs = Setting.PixelPerMs;
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
