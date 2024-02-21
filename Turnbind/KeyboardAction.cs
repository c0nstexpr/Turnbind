namespace Turnbind
{
    using Stateless;
    using Stateless.Graph;

    using System.Windows;

    internal class KeyboardAction
    {
        enum State
        {
            OffHook,
            Ringing,
            Connected,
            OnHold,
            PhoneDestroyed
        }

        public KeyboardAction()
        {
            var phoneCall = new StateMachine<State, Trigger>(State.OffHook);
        }
    }
}
