using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ObservableCollections;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableHashSet<ProfileNameItemViewModel> m_profilesNames = [];

    [ObservableProperty]
    string? m_textBoxProfileName;

    bool CanAddProfileName() => TextBoxProfileName is { };

    [RelayCommand(CanExecute = nameof(CanAddProfileName))]
    void AddProfileName()
    {
        Debug.Assert(TextBoxProfileName is { });
        if (ProfilesNames.Add(new() { ProfileName = TextBoxProfileName }))
            TextBoxProfileName = null;
    }
}
