using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Helper;

namespace Turnbind.ViewModel;

partial class BindEditViewModel : ObservableObject, IDisposable
{
    readonly SerialDisposable m_keysDisposable = new();

    InputKeysViewModel m_inputKeys = new();

    public InputKeysViewModel InputKeys
    {
        get => m_inputKeys;

        set
        {
            SetProperty(ref m_inputKeys, value);
            m_keysDisposable.Disposable = value.WhenChanged(x => x.Keys).Subscribe(
                keys => RemoveCommand.NotifyCanExecuteChanged());
        }
    }

    TurnSettingViewModel m_turnSetting = new();

    public TurnSettingViewModel TurnSetting
    {
        get => m_turnSetting;
        set => SetProperty(ref m_turnSetting, value);
    }

    RelayCommand m_modifyCommand = RelayCommandExt.DefaultCommand;

    public RelayCommand ModifyCommand
    {
        get => m_modifyCommand;
        set => SetProperty(ref m_modifyCommand, value);
    }

    RelayCommand m_removeCommand = RelayCommandExt.DefaultCommand;

    public RelayCommand RemoveCommand
    {
        get => m_removeCommand;
        set => SetProperty(ref m_removeCommand, value);
    }

    public BindEditViewModel() => TurnSetting = new();

    public void Dispose() => m_keysDisposable.Dispose();
}
