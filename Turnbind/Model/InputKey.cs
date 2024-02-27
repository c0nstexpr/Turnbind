using SharpHook.Native;

namespace Turnbind.Model;

public enum InputKey
{
    None = 0,
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z,

    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9,

    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,
    F13,
    F14,
    F15,
    F16,
    F17,
    F18,
    F19,
    F20,
    F21,
    F22,
    F23,
    F24,

    Escape,

    BackQuote,
    Minus,
    Equals,
    Backspace,

    Tab,
    LeftBracket,
    RightBracket,
    Backslash,

    CapsLock,
    Semicolon,
    Quote,
    Enter,

    LeftShift,
    Comma,
    Period,
    Slash,
    RightShift,

    LeftCtrl,
    LeftAlt,
    Space,
    RightAlt,
    Function,
    ContextMenu,
    RightCtrl,

    Insert,
    Home,
    PageUp,
    Delete,
    End,
    PageDown,

    Up,
    Left,
    Down,
    Right,

    NumLock,
    NumDivide,
    NumMultiply,
    NumSubtract,
    NumAdd,
    NumSeparator,
    Num0,
    Num1,
    Num2,
    Num3,
    Num4,
    Num5,
    Num6,
    Num7,
    Num8,
    Num9,
    NumEnter,

    MouseLeft,
    MouseMiddle,
    MouseRight,
    MouseX1,
    MouseX2,
    MouseX3,
    MouseX4,
    MouseX5,
    MouseWheelUp,
    MouseWheelDown
}

public static class InputKeyExt
{
    public static string ToKeyString(this IEnumerable<InputKey> keys) => string.Join(" + ", keys.Select(k => $"{k}"));

    public static IReadOnlyDictionary<InputKey, KeyCode> KeyCodeMap { get; } = new Dictionary<InputKey, KeyCode>
    {
        [InputKey.None] = KeyCode.VcUndefined,
        [InputKey.A] = KeyCode.VcA,
        [InputKey.B] = KeyCode.VcB,
        [InputKey.C] = KeyCode.VcC,
        [InputKey.D] = KeyCode.VcD,
        [InputKey.E] = KeyCode.VcE,
        [InputKey.F] = KeyCode.VcF,
        [InputKey.G] = KeyCode.VcG,
        [InputKey.H] = KeyCode.VcH,
        [InputKey.I] = KeyCode.VcI,
        [InputKey.J] = KeyCode.VcJ,
        [InputKey.K] = KeyCode.VcK,
        [InputKey.L] = KeyCode.VcL,
        [InputKey.M] = KeyCode.VcM,
        [InputKey.N] = KeyCode.VcN,
        [InputKey.O] = KeyCode.VcO,
        [InputKey.P] = KeyCode.VcP,
        [InputKey.Q] = KeyCode.VcQ,
        [InputKey.R] = KeyCode.VcR,
        [InputKey.S] = KeyCode.VcS,
        [InputKey.T] = KeyCode.VcT,
        [InputKey.U] = KeyCode.VcU,
        [InputKey.V] = KeyCode.VcV,
        [InputKey.W] = KeyCode.VcW,
        [InputKey.X] = KeyCode.VcX,
        [InputKey.Y] = KeyCode.VcY,
        [InputKey.Z] = KeyCode.VcZ,

        [InputKey.D0] = KeyCode.Vc0,
        [InputKey.D1] = KeyCode.Vc1,
        [InputKey.D2] = KeyCode.Vc2,
        [InputKey.D3] = KeyCode.Vc3,
        [InputKey.D4] = KeyCode.Vc4,
        [InputKey.D5] = KeyCode.Vc5,
        [InputKey.D6] = KeyCode.Vc6,
        [InputKey.D7] = KeyCode.Vc7,
        [InputKey.D8] = KeyCode.Vc8,
        [InputKey.D9] = KeyCode.Vc9,
        [InputKey.F1] = KeyCode.VcF1,
        [InputKey.F2] = KeyCode.VcF2,
        [InputKey.F3] = KeyCode.VcF3,
        [InputKey.F4] = KeyCode.VcF4,
        [InputKey.F5] = KeyCode.VcF5,
        [InputKey.F6] = KeyCode.VcF6,
        [InputKey.F7] = KeyCode.VcF7,
        [InputKey.F8] = KeyCode.VcF8,
        [InputKey.F9] = KeyCode.VcF9,

        [InputKey.F10] = KeyCode.VcF10,
        [InputKey.F11] = KeyCode.VcF11,
        [InputKey.F12] = KeyCode.VcF12,
        [InputKey.F13] = KeyCode.VcF13,
        [InputKey.F14] = KeyCode.VcF14,
        [InputKey.F15] = KeyCode.VcF15,
        [InputKey.F16] = KeyCode.VcF16,
        [InputKey.F17] = KeyCode.VcF17,
        [InputKey.F18] = KeyCode.VcF18,
        [InputKey.F19] = KeyCode.VcF19,
        [InputKey.F20] = KeyCode.VcF20,
        [InputKey.F21] = KeyCode.VcF21,
        [InputKey.F22] = KeyCode.VcF22,
        [InputKey.F23] = KeyCode.VcF23,
        [InputKey.F24] = KeyCode.VcF24,

        [InputKey.Escape] = KeyCode.VcEscape,
        [InputKey.BackQuote] = KeyCode.VcBackQuote,
        [InputKey.Minus] = KeyCode.VcMinus,
        [InputKey.Equals] = KeyCode.VcEquals,
        [InputKey.Backspace] = KeyCode.VcBackspace,

        [InputKey.Tab] = KeyCode.VcTab,
        [InputKey.LeftBracket] = KeyCode.VcOpenBracket,
        [InputKey.RightBracket] = KeyCode.VcCloseBracket,
        [InputKey.Backslash] = KeyCode.VcBackslash,

        [InputKey.CapsLock] = KeyCode.VcCapsLock,
        [InputKey.Semicolon] = KeyCode.VcSemicolon,
        [InputKey.Quote] = KeyCode.VcQuote,
        [InputKey.Enter] = KeyCode.VcEnter,

        [InputKey.LeftShift] = KeyCode.VcLeftShift,
        [InputKey.Comma] = KeyCode.VcComma,
        [InputKey.Period] = KeyCode.VcPeriod,
        [InputKey.Slash] = KeyCode.VcSlash,
        [InputKey.RightShift] = KeyCode.VcRightShift,

        [InputKey.LeftCtrl] = KeyCode.VcLeftControl,
        [InputKey.LeftAlt] = KeyCode.VcLeftAlt,
        [InputKey.Space] = KeyCode.VcSpace,
        [InputKey.RightAlt] = KeyCode.VcRightAlt,
        [InputKey.Function] = KeyCode.VcFunction,
        [InputKey.ContextMenu] = KeyCode.VcContextMenu,
        [InputKey.RightCtrl] = KeyCode.VcRightControl,

        [InputKey.Insert] = KeyCode.VcInsert,
        [InputKey.Home] = KeyCode.VcHome,
        [InputKey.PageUp] = KeyCode.VcPageUp,
        [InputKey.Delete] = KeyCode.VcDelete,
        [InputKey.End] = KeyCode.VcEnd,
        [InputKey.PageDown] = KeyCode.VcPageDown,

        [InputKey.Up] = KeyCode.VcUp,
        [InputKey.Left] = KeyCode.VcLeft,
        [InputKey.Down] = KeyCode.VcDown,
        [InputKey.Right] = KeyCode.VcRight,

        [InputKey.NumLock] = KeyCode.VcNumLock,
        [InputKey.NumDivide] = KeyCode.VcNumPadDivide,
        [InputKey.NumMultiply] = KeyCode.VcNumPadMultiply,
        [InputKey.NumSubtract] = KeyCode.VcNumPadSubtract,
        [InputKey.NumAdd] = KeyCode.VcNumPadAdd,
        [InputKey.NumSeparator] = KeyCode.VcNumPadSeparator,
        [InputKey.Num0] = KeyCode.VcNumPad0,
        [InputKey.Num1] = KeyCode.VcNumPad1,
        [InputKey.Num2] = KeyCode.VcNumPad2,
        [InputKey.Num3] = KeyCode.VcNumPad3,
        [InputKey.Num4] = KeyCode.VcNumPad4,
        [InputKey.Num5] = KeyCode.VcNumPad5,
        [InputKey.Num6] = KeyCode.VcNumPad6,
        [InputKey.Num7] = KeyCode.VcNumPad7,
        [InputKey.Num8] = KeyCode.VcNumPad8,
        [InputKey.Num9] = KeyCode.VcNumPad9,
        [InputKey.NumEnter] = KeyCode.VcNumPadEnter
    };

    public static IReadOnlyDictionary<KeyCode, InputKey> KeyCodeMapInv { get; } = KeyCodeMap.ToInvDictionary();

    public static KeyCode ToKeyCode(this InputKey key) =>
        KeyCodeMap.TryGetValue(key, out var keyCode) ? keyCode : KeyCode.VcUndefined;

    public static InputKey ToInputKey(this KeyCode keyCode) => KeyCodeMapInv.GetValueOrDefault(keyCode);

    public static IReadOnlyDictionary<InputKey, MouseButton> MouseButtonMap { get; } = new Dictionary<InputKey, MouseButton>
    {
        [InputKey.None] = MouseButton.NoButton,
        [InputKey.MouseLeft] = MouseButton.Button1,
        [InputKey.MouseMiddle] = MouseButton.Button3,
        [InputKey.MouseRight] = MouseButton.Button2,
        [InputKey.MouseX1] = MouseButton.Button4,
        [InputKey.MouseX2] = MouseButton.Button5,
        [InputKey.MouseX3] = MouseButton.Button5 + 1,
        [InputKey.MouseX4] = MouseButton.Button5 + 2,
        [InputKey.MouseX5] = MouseButton.Button5 + 3
    };

    public static IReadOnlyDictionary<MouseButton, InputKey> MouseButtonMapInv { get; } =
        MouseButtonMap.ToInvDictionary();

    public static MouseButton ToMouseButton(this InputKey key) =>
        MouseButtonMap.GetValueOrDefault(key);

    public static InputKey ToInputKey(this MouseButton button) =>
        MouseButtonMapInv.GetValueOrDefault(button);
}
