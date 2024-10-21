using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Helper;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject, IDisposable
{
    readonly Dictionary<string, ProfileItemViewModel> m_profiles = [];

    readonly Key2IndexCollection<string> m_profileIndices = [];

    readonly ObservableCollection<ProfileItemViewModel> m_itemSource = [];

    public INotifyCollectionChanged ItemSource => m_itemSource;

    string? m_inputBoxProfileName;

    public string? InputProfile
    {
        get => m_inputBoxProfileName;

        set
        {
            SetProperty(ref m_inputBoxProfileName, value);
            AddProfileCommand.NotifyCanExecuteChanged();
        }
    }

    readonly BehaviorSubject<string?> m_viewItem = new(null);

    public IObservable<string?> ViewItem => m_viewItem;

    readonly Subject<string> m_addItem = new();

    public IObservable<string> AddItem => m_addItem;

    readonly Subject<string> m_removeItem = new();

    public IObservable<string> RemoveItem => m_removeItem;

    bool CanAddProfile() => InputProfile is { } n && !m_profiles.ContainsKey(n);

    [RelayCommand(CanExecute = nameof(CanAddProfile))]
    void AddProfile()
    {
        AddCore(InputProfile!);
        InputProfile = null;
    }

    public bool Add(string name)
    {
        if (m_profiles.ContainsKey(name)) return false;

        AddCore(name);
        return true;
    }

    public bool Remove(string name)
    {
        if (!m_profiles.ContainsKey(name)) return false;

        RemoveCore(name);
        return true;
    }

    void AddCore(string name)
    {
        ProfileItemViewModel vm = new()
        {
            Name = name,
            ViewCmd = new(() => m_viewItem.OnNext(name)),
            RemoveCmd = new(() => RemoveCore(name))
        };
        m_profiles[name] = vm;
        m_itemSource.Add(vm);
        m_profileIndices.Add(name);

        m_addItem.OnNext(name);
    }

    void RemoveCore(string name)
    {
        var i = m_profileIndices[name];

        m_profiles.Remove(name);
        m_itemSource.RemoveAt(i);
        m_profileIndices.Remove(name);

        m_removeItem.OnNext(name);

        if(m_viewItem.Value == name) m_viewItem.OnNext(null);
    }

    public void Dispose()
    {
        m_viewItem.Dispose();
        m_addItem.Dispose();
        m_removeItem.Dispose();
    }
}
