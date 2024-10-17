using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Turnbind.ViewModel;

sealed partial class ProfileItemViewModel : ObservableObject
{
    [ObservableProperty]
    string m_name = string.Empty;

    [ObservableProperty]
    bool m_enabled;

    RelayCommand m_viewCmd;

    public required RelayCommand ViewCmd
    {
        get => m_viewCmd;

        [MemberNotNull(nameof(m_viewCmd))]
        set => SetProperty(ref m_viewCmd, value);
    }

    RelayCommand m_removeCmd;

    public required RelayCommand RemoveCmd
    {
        get => m_removeCmd;

        [MemberNotNull(nameof(m_removeCmd))]
        set => SetProperty(ref m_removeCmd, value);
    }
}
