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

    public void Enable()
    {
        Disable();

        IDisposable? focusedDisposable = null;

        void OnFocuse(bool focused)
        {
            if (!focused)
            {
                OnActive(false);
                focusedDisposable?.Dispose();
                focusedDisposable = null;
                return;
            }

            if (focusedDisposable is { })
            {
                OnActive(true);
                return;
            }

            void OnActive(bool active)
            {
                var turnAction = App.GetService<TurnAction>();

                if (!active)
                {
                    turnAction.Direction = TurnInstruction.Stop;
                    return;
                }

                turnAction.Direction = m_dir;
                turnAction.PixelPerSec = Setting.PixelPerSec;
            }

            focusedDisposable = App.GetService<InputAction>().SubscribeKeys(Keys).Subscribe(OnActive);
        }

        m_disposble = new CompositeDisposable()
        {
            Disposable.Create(() => OnFocuse(false)),
            App.GetService<ProcessWindowAction>().Focused.Subscribe(OnFocuse)
        };
    }

    public void Disable()
    {
        m_disposble?.Dispose();
        m_disposble = null;
    }

    public void Dispose() => Disable();
}
