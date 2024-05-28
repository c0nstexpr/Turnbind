using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Turnbind.Action;
using Turnbind.Model;
using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class KeyBindEditControl : UserControl, IDisposable
{
    internal readonly KeyBindEditViewModel m_viewModel = new();

    public KeyBindEditControl()
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
                m_viewModel.KeyBind.Keys = [];
                break;

            default:
                m_viewModel.OnInputKey(key);
                break;
        }
    }

    readonly SerialDisposable m_keyboardDisposable = new();

    void InputKeysTextBoxFocus(object sender, RoutedEventArgs e) => 
        m_keyboardDisposable.Disposable = App.GetRequiredService<InputAction>()
            .KeysInput
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
