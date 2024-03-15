using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MoreLinq;

using ObservableCollections;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject, IDisposable
{
    public readonly ObservableHashSet<ProfileNameItemViewModel> ProfilesNames = [];

    readonly Dictionary<string, IDisposable> m_profileDisposable = [];

    string? m_textBoxProfileName;

    public string? TextBoxProfileName
    {
        get => m_textBoxProfileName;
        set
        {
            SetProperty(ref m_textBoxProfileName, value);
            AddProfileNameCommand.NotifyCanExecuteChanged();
        }
    }

    bool CanAddProfileName() => TextBoxProfileName is { };

    [RelayCommand(CanExecute = nameof(CanAddProfileName))]
    void AddProfileName()
    {
        Debug.Assert(TextBoxProfileName is { });
        ProfileNameItemViewModel item = new() { Name = TextBoxProfileName };

        if (!ProfilesNames.Add(item))
        {
            item.Dispose();
            return;
        }

        m_profileDisposable[item.Name] = item.RemoveProfile.Subscribe(
            _ =>
            {
                ProfilesNames.Remove(item);
                item.Dispose();
                m_profileDisposable[item.Name].Dispose();
                m_profileDisposable.Remove(item.Name);
            }
        );

        TextBoxProfileName = null;
    }

    public void Dispose()
    {
        m_profileDisposable.Values.ForEach(item => item.Dispose());
        ProfilesNames.ForEach(item => item.Dispose());
    }
}
