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

    void InputKeysTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (m_isFocusFirstInput)
        {
            m_isFocusFirstInput = false;
            return;
        }

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
                m_viewModel.OnInputKey(
                    e.Key switch
                    #region
                    {
                        Key.Back => InputKey.Backspace,
                        Key.Tab => InputKey.Tab,
                        Key.Enter => InputKey.Enter,
                        Key.CapsLock => InputKey.CapsLock,
                        Key.Escape => InputKey.Escape,
                        Key.Space => InputKey.Space,
                        Key.PageUp => InputKey.PageUp,
                        Key.PageDown => InputKey.PageDown,
                        Key.End => InputKey.End,
                        Key.Home => InputKey.Home,
                        Key.Left => InputKey.Left,
                        Key.Up => InputKey.Up,
                        Key.Right => InputKey.Right,
                        Key.Down => InputKey.Down,
                        Key.Insert => InputKey.Insert,
                        Key.Delete => InputKey.Delete,
                        Key.D0 => InputKey.D0,
                        Key.D1 => InputKey.D1,
                        Key.D2 => InputKey.D2,
                        Key.D3 => InputKey.D3,
                        Key.D4 => InputKey.D4,
                        Key.D5 => InputKey.D5,
                        Key.D6 => InputKey.D6,
                        Key.D7 => InputKey.D7,
                        Key.D8 => InputKey.D8,
                        Key.D9 => InputKey.D9,
                        Key.A => InputKey.A,
                        Key.B => InputKey.B,
                        Key.C => InputKey.C,
                        Key.D => InputKey.D,
                        Key.E => InputKey.E,
                        Key.F => InputKey.F,
                        Key.G => InputKey.G,
                        Key.H => InputKey.H,
                        Key.I => InputKey.I,
                        Key.J => InputKey.J,
                        Key.K => InputKey.K,
                        Key.L => InputKey.L,
                        Key.M => InputKey.M,
                        Key.N => InputKey.N,
                        Key.O => InputKey.O,
                        Key.P => InputKey.P,
                        Key.Q => InputKey.Q,
                        Key.R => InputKey.R,
                        Key.S => InputKey.S,
                        Key.T => InputKey.T,
                        Key.U => InputKey.U,
                        Key.V => InputKey.V,
                        Key.W => InputKey.W,
                        Key.X => InputKey.X,
                        Key.Y => InputKey.Y,
                        Key.Z => InputKey.Z,
                        Key.NumPad0 => InputKey.Num0,
                        Key.NumPad1 => InputKey.Num1,
                        Key.NumPad2 => InputKey.Num2,
                        Key.NumPad3 => InputKey.Num3,
                        Key.NumPad4 => InputKey.Num4,
                        Key.NumPad5 => InputKey.Num5,
                        Key.NumPad6 => InputKey.Num6,
                        Key.NumPad7 => InputKey.Num7,
                        Key.NumPad8 => InputKey.Num8,
                        Key.NumPad9 => InputKey.Num9,
                        Key.Multiply => InputKey.NumMultiply,
                        Key.Add => InputKey.NumAdd,
                        Key.Subtract => InputKey.NumSubtract,
                        Key.Decimal => InputKey.NumSeparator,
                        Key.Divide => InputKey.NumDivide,
                        Key.F1 => InputKey.F1,
                        Key.F2 => InputKey.F2,
                        Key.F3 => InputKey.F3,
                        Key.F4 => InputKey.F4,
                        Key.F5 => InputKey.F5,
                        Key.F6 => InputKey.F6,
                        Key.F7 => InputKey.F7,
                        Key.F8 => InputKey.F8,
                        Key.F9 => InputKey.F9,
                        Key.F10 => InputKey.F10,
                        Key.F11 => InputKey.F11,
                        Key.F12 => InputKey.F12,
                        Key.F13 => InputKey.F13,
                        Key.F14 => InputKey.F14,
                        Key.F15 => InputKey.F15,
                        Key.F16 => InputKey.F16,
                        Key.F17 => InputKey.F17,
                        Key.F18 => InputKey.F18,
                        Key.F19 => InputKey.F19,
                        Key.F20 => InputKey.F20,
                        Key.F21 => InputKey.F21,
                        Key.F22 => InputKey.F22,
                        Key.F23 => InputKey.F23,
                        Key.F24 => InputKey.F24,
                        Key.NumLock => InputKey.NumLock,
                        Key.Scroll => InputKey.ScrollLock,
                        Key.LeftShift => InputKey.LeftShift,
                        Key.RightShift => InputKey.RightShift,
                        Key.LeftCtrl => InputKey.LCtrl,
                        Key.RightCtrl => InputKey.RCtrl,
                        Key.LeftAlt => InputKey.LAlt,
                        Key.RightAlt => InputKey.RAlt,
                        Key.OemSemicolon => InputKey.Semicolon,
                        Key.OemPlus => InputKey.Equals,
                        Key.OemComma => InputKey.Comma,
                        Key.OemMinus => InputKey.Minus,
                        Key.OemPeriod => InputKey.Period,
                        Key.OemQuestion => InputKey.Slash,
                        Key.OemTilde => InputKey.BackQuote,
                        Key.OemOpenBrackets => InputKey.LeftBracket,
                        Key.OemCloseBrackets => InputKey.RightBracket,
                        Key.OemPipe => InputKey.Backslash,
                        Key.OemQuotes => InputKey.Quote,
                        _ => GetLatestPressedKey()
                    }
                    #endregion
                );
                break;
        }

        e.Handled = true;
    }

    static InputKey GetLatestPressedKey()
    {
        var latest = App.GetService<InputAction>().LatestKeyState;
        return latest.Pressed ? latest.Key : InputKey.None;
    }

    void InputKeysTextBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (m_isFocusFirstInput)
        {
            m_isFocusFirstInput = false;
            return;
        }

        m_viewModel.OnInputKey(
            e.ChangedButton switch
            {
                MouseButton.Left => InputKey.MouseLeft,
                MouseButton.Middle => InputKey.MouseMiddle,
                MouseButton.Right => InputKey.MouseRight,
                MouseButton.XButton1 => InputKey.MouseX1,
                MouseButton.XButton2 => InputKey.MouseX2,
                _ => GetLatestPressedKey()
            }
        );

        e.Handled = true;
    }

    public void Dispose() => m_viewModel.Dispose();

    bool m_isFocusFirstInput = true;

    void InputKeysTextBoxLostFocus(object sender, RoutedEventArgs e) => m_isFocusFirstInput = true;
}
