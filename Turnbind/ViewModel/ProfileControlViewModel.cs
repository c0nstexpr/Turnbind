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
        ProfileNameItemViewModel item = new() { Name = TextBoxProfileName };

        if (!ProfilesNames.Add(item)) return;

        TextBoxProfileName = null;
    }
}
