using CommunityToolkit.Mvvm.ComponentModel;

namespace Turnbind.ViewModel;

partial class BindsItemViewModel : ObservableValidator
{
    [ObservableProperty]
    InputKeysViewModel m_inputKeys = [];

    [ObservableProperty]
    TurnSettingViewModel m_turnSetting = new();
}
