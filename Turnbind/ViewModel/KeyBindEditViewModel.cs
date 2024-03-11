using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindEditViewModel : ObservableObject
{
    public readonly RelayCommand DefaultCommand = new(() => { }, () => false);

    [ObservableProperty]
    KeyBindViewModel m_keyBind = new();

    public void OnInputKey(InputKey k)
    {
        if (KeyBind.Keys.Contains(k)) return;

        KeyBind.Keys = new(KeyBind.Keys.Concat([k]));
    }

    [ObservableProperty]
    RelayCommand m_addCommand;

    [ObservableProperty]
    RelayCommand m_modifyCommand;

    [ObservableProperty]
    RelayCommand m_removeCommand;

    public KeyBindEditViewModel()
    {
        m_addCommand = DefaultCommand;
        m_modifyCommand = DefaultCommand;
        m_removeCommand = DefaultCommand;
    }
}
