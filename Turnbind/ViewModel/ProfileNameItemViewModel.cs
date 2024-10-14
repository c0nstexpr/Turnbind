using CommunityToolkit.Mvvm.ComponentModel;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject
{
    [ObservableProperty]
    string m_name = string.Empty;

    [ObservableProperty]
    bool m_enabled;
}
