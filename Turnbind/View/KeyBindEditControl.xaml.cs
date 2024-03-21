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

    void InputKeysTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key;
        var textBlock = (Wpf.Ui.Controls.TextBox)sender;

        switch (key)
        {
            case Key.Tab:
                textBlock.MoveFocus(new(FocusNavigationDirection.Next));
                break;

            case Key.Enter:
                textBlock.MoveFocus(new(FocusNavigationDirection.Next));
                break;

            case Key.Escape:
                textBlock.MoveFocus(new(FocusNavigationDirection.Previous));
                break;

            case Key.Back:
                m_viewModel.KeyBind.Keys = [];
                break;

            default:
                m_viewModel.OnInputKey(key == Key.None ? GetLatestPressedKey() : (InputKey)KeyInterop.VirtualKeyFromKey(key));
                break;
        }

        e.Handled = true;
    }
    static InputKey GetLatestPressedKey()
    {
        var latest = App.GetService<InputAction>().LatestKeyState;
        return latest.Pressed ? latest.Key : InputKey.None;
    }

    public void Dispose() => m_viewModel.Dispose();
}
