using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ObservableCollections;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject
{
    public readonly ObservableHashSet<ProfileNameItemViewModel> ProfilesNames = [];

    [ObservableProperty]
    string? m_textBoxProfileName;

    [ObservableProperty]
    ProfileNameItemViewModel? m_selectedProfileName;

    [RelayCommand]
    public void AddProfileName()
    {
        if (TextBoxProfileName is null) return;

        if (ProfilesNames.Add(new() { ProfileName = TextBoxProfileName }))
            TextBoxProfileName = null;
    }

    bool CanRemoveProfileName() => SelectedProfileName is { };

    [RelayCommand(CanExecute = nameof(CanRemoveProfileName))]
    public void RemoveProfileName()
    {
        Debug.Assert(SelectedProfileName is { });
        if (ProfilesNames.Remove(SelectedProfileName))
            SelectedProfileName = null;
    }
}
