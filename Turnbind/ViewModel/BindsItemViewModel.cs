using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Turnbind.ViewModel;

partial class BindsItemViewModel : ObservableValidator
{
    [ObservableProperty]
    InputKeysViewModel m_inputKeys = new();

    [ObservableProperty]
    TurnSettingViewModel m_turnSetting = new();
}
