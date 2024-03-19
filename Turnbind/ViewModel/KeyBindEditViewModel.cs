using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindEditViewModel : ObservableObject, IDisposable
{
    public readonly RelayCommand DefaultCommand = new(() => { }, () => false);

    readonly SerialDisposable m_keyBindDisposable = new();

    KeyBindViewModel m_keyBind;

    public KeyBindViewModel KeyBind
    {
        get => m_keyBind;

        [MemberNotNull(nameof(m_keyBind))]
        set
        {
            SetProperty(ref m_keyBind, value);

            AddCommand.NotifyCanExecuteChanged();
            RemoveCommand.NotifyCanExecuteChanged();
            ModifyCommand.NotifyCanExecuteChanged();

            m_keyBindDisposable.Disposable = value.WhenChanged(x => x.Keys).Subscribe(
                _ =>
                {
                    AddCommand.NotifyCanExecuteChanged();
                    RemoveCommand.NotifyCanExecuteChanged();
                    ModifyCommand.NotifyCanExecuteChanged();
                }
            );
        }
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
        KeyBind = new();
    }

    public void OnInputKey(InputKey k)
    {
        if (KeyBind.Keys.Contains(k)) return;

        KeyBind.Keys = new(KeyBind.Keys.Concat([k]));
    }

    public void Dispose() => m_keyBindDisposable.Dispose();
}
