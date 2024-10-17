using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Turnbind.Action;
using Turnbind.Model;
using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class BindsControl : UserControl, IDisposable
{
    internal readonly BindsViewModel m_viewModel = new();

    readonly Lazy<InputAction> m_inputAction = new(App.GetRequiredService<InputAction>);

    public BindsControl()
    {
        InitializeComponent();
        DataContext = m_viewModel;
    }

    void InputKeysTextBoxKeyDown(InputKey key)
    {
        switch (key)
        {
            case InputKey.Tab:
                InputKeysTextBox.MoveFocus(new(FocusNavigationDirection.Next));
                break;

            case InputKey.Enter:
                InputKeysTextBox.MoveFocus(new(FocusNavigationDirection.Next));
                break;

            case InputKey.Escape:
                InputKeysTextBox.MoveFocus(new(FocusNavigationDirection.Previous));
                break;

            case InputKey.Back:
                m_viewModel.Selected.InputKeys = [];
                break;

            default:
                m_viewModel.Selected.InputKeys.OnInputKey(key);
                break;
        }
    }

    readonly SerialDisposable m_keyboardDisposable = new();

    void InputKeysTextBoxFocus(object sender, RoutedEventArgs e) =>
        m_keyboardDisposable.Disposable = m_inputAction.Value.KeyboardInput
            .Where(state => state.Pressed)
            .Select(state => state.Key)
            .Subscribe(InputKeysTextBoxKeyDown);

    void InputKeysTextBoxLostFocus(object sender, RoutedEventArgs e) => m_keyboardDisposable.Disposable = null;

    public void Dispose()
    {
        m_keyboardDisposable.Dispose();
        m_viewModel.Dispose();
    }
}
