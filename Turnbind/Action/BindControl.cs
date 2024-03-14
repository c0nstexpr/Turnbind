using System.Reactive.Disposables;

using Turnbind.Model;

namespace Turnbind.Action;

class BindControl : IDisposable
{
    public InputKeys Keys { get; }

    public TurnSetting Setting { get; }

    readonly TurnAction.Instruction m_dir;

    IDisposable? m_disposble;

    public BindControl(InputKeys keys, TurnSetting setting)
    {
        Keys = keys;
        Setting = setting;
        m_dir = Setting.Dir switch
        {
            TurnDirection.Left => TurnAction.Instruction.Left,
            TurnDirection.Right => TurnAction.Instruction.Right,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

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
                    turnAction.Direction = TurnAction.Instruction.Stop;
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
