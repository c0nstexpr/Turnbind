using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindEditViewModel : ObservableObject, IDisposable
{
    public readonly RelayCommand DefaultCommand = new(() => { }, () => false);

    IDisposable m_keyBindDisposable = Disposable.Empty;

    KeyBindViewModel m_keyBind = new();

    public KeyBindViewModel KeyBind
    {
        get => m_keyBind;

        set
        {
            SetProperty(ref m_keyBind, value);

            AddCommand.NotifyCanExecuteChanged();
            RemoveCommand.NotifyCanExecuteChanged();
            ModifyCommand.NotifyCanExecuteChanged();

            m_keyBindDisposable = value.WhenChanged(x => x.Keys).Subscribe(
                _ =>
                {
                    AddCommand.NotifyCanExecuteChanged();
                    RemoveCommand.NotifyCanExecuteChanged();
                    ModifyCommand.NotifyCanExecuteChanged();
                }
            );
        }
    }

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

    public void Dispose() => m_keyBindDisposable.Dispose();
}
