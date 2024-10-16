using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Turnbind.Action;
using Turnbind.Model;
using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class BindEditControl : UserControl, IDisposable
{
    internal readonly BindEditViewModel m_viewModel = new();

    readonly Lazy<InputAction> m_inputAction = new(App.GetRequiredService<InputAction>);

    public BindEditControl()
    {
        DataContext = m_viewModel;
        InitializeComponent();
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
                m_viewModel.InputKeys = new();
                break;

            default:
                m_viewModel.InputKeys.OnInputKey(key);
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
