using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MoreLinq;

using ObservableCollections;

using Turnbind.Helper;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject, IDisposable
{
    readonly ObservableDictionary<string, ProfileNameItemViewModel> m_profilesNames = [];

    internal IObservableCollection<KeyValuePair<string, ProfileNameItemViewModel>> m_observableProfilesNames => m_profilesNames;

    readonly Dictionary<string, IDisposable> m_profileDisposable = [];

    readonly ObservableDictionaryListView<string, ProfileNameItemViewModel> m_profilesNamesView;

    public ObservableDictionaryListView<string, ProfileNameItemViewModel>.ValueCollectionChanged ProfilesNames { get; }

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

    public ProfileControlViewModel()
    {
        m_profilesNamesView = new(m_profilesNames);
        ProfilesNames = m_profilesNamesView.CreateValueView();
    }

    bool CanAddProfileName() => TextBoxProfileName is { };

    public ProfileNameItemViewModel? Add(string name)
    {
        Debug.Assert(name is { });
        ProfileNameItemViewModel item = new() { Name = name };

        if (!m_profilesNames.TryAdd(name, item))
        {
            item.Dispose();
            return null;
        }

        m_profileDisposable[name] = item.RemoveProfile.Subscribe(index => Remove(name));

        return item;
    }

    public void Remove(string name)
    {
        m_profileDisposable[name].Dispose();
        m_profileDisposable.Remove(name);
        m_profilesNames[name].Dispose();
        m_profilesNames.Remove(name);
    }

    public void Clear()
    {
        m_profileDisposable.Values.ForEach(item => item.Dispose());
        m_profileDisposable.Clear();
        (m_profilesNames as IDictionary<string, ProfileNameItemViewModel>).Values
            .ForEach(item => item.Dispose());
        m_profilesNames.Clear();
    }

    [RelayCommand(CanExecute = nameof(CanAddProfileName))]
    void AddProfileName()
    {
        Add(TextBoxProfileName!);
        TextBoxProfileName = null;
    }

    public void Dispose()
    {
        m_profilesNamesView.Dispose();
        ProfilesNames.Dispose();
        m_profileDisposable.Values.ForEach(item => item.Dispose());
        (m_profilesNames as IDictionary<string, ProfileNameItemViewModel>).Values
            .ForEach(item => item.Dispose());
    }
}
