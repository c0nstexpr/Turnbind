using System.Collections.ObjectModel;
using ReactiveMarbles.ObservableEvents;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject
{
    readonly HashSet<string> m_profilesNames = [];

    public readonly ObservableCollection<string> ProfilesNames = [];

    [ObservableProperty]
    string m_activeProfileName = Settings.DefaultProfileName;

    [ObservableProperty]
    bool m_addButtonEnable = false;

    [ObservableProperty]
    bool m_removeButtonEnable = false;

    string? m_textBoxProfileName;

    public string? TextBoxProfileName
    {
        get => m_textBoxProfileName;

        set
        {
            SetProperty(ref m_textBoxProfileName, value);

            AddButtonEnable = value is { } && m_profilesNames.Contains(value);
        }
    }

    string? m_selectedProfileName;

    public string? SelectedProfileName
    {
        get => m_selectedProfileName;

        set
        {
            SetProperty(ref m_selectedProfileName, value);

            RemoveButtonEnable = value is { } && m_profilesNames.Contains(value);
        }
    }

    public void OnProfileNameListSelected() => ActiveProfileName = SelectedProfileName!;

    public void OnAddProfileName()
    {
        if (TextBoxProfileName is null || !m_profilesNames.Add(TextBoxProfileName)) return;

        ProfilesNames.Add(TextBoxProfileName);
        TextBoxProfileName = null;
    }

    public void OnRemoveProfileName()
    {
        if (SelectedProfileName is null || !m_profilesNames.Remove(SelectedProfileName)) return;

        ProfilesNames.Remove(SelectedProfileName);
        SelectedProfileName = null;
    }
}
