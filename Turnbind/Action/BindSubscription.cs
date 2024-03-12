using System.Reactive.Disposables;
using System.Reactive.Subjects;

using Turnbind.Model;

namespace Turnbind.Action;

class BindSubscription : IDisposable
{
    public InputKeys Keys { get; }

    public TurnSetting Setting { get; }

    readonly TurnAction.Instruction m_dir;

    IDisposable? m_disposble;

    Subject<bool> m_active = new();

    public BindSubscription(InputKeys keys, TurnSetting setting)
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

    public void Enable(ProcessWindowAction windowAction, InputAction inputAction, TurnAction turnAction)
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
                if (!active)
                {
                    turnAction.Direction = TurnAction.Instruction.Stop;
                    return;
                }

                turnAction.Direction = m_dir;
                turnAction.PixelPerSec = Setting.PixelPerSec;
            }

            focusedDisposable = inputAction.SubscribeKeys(Keys).Subscribe(OnActive);
        }

        m_disposble = new CompositeDisposable()
        {
            Disposable.Create(() => OnFocuse(false)),
            windowAction.Focused.Subscribe(OnFocuse)
        };
    }

    public void Disable()
    {
        m_disposble?.Dispose();
        m_disposble = null;
    }

    public void Dispose() => Disable();
}
