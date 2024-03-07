using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;
using Turnbind.View;

namespace Turnbind.ViewModel;

partial class KeyBindViewModel : ObservableObject
{
    InputKeys m_keys = new([]);

    public InputKeys Keys
    {
        get => m_keys;

        set
        {
            SetProperty(ref m_keys, value);
            OnPropertyChanged(nameof(KeysString));
        }
    }

    public string KeysString => m_keys.ToKeyString();

    [ObservableProperty]
    TurnSettingViewModel m_turnSetting = new();
}
