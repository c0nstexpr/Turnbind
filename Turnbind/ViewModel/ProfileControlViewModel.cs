using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ObservableCollections;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject
{
    public readonly ObservableHashSet<ProfileNameItemViewModel> ProfilesNames = [];

    readonly Dictionary<ProfileNameItemViewModel, IDisposable> m_profileNameItemSubscriptions = [];

    [ObservableProperty]
    string? m_textBoxProfileName;

    [ObservableProperty]
    ProfileNameItemViewModel? m_selectedProfileName;

    readonly Subject<(string ProfileName, bool Enable)> m_profileNameEnable = new();

    public IObservable<(string, bool)> ProfileNameEnable => m_profileNameEnable;

    public ProfileControlViewModel() => ProfilesNames.CollectionChanged += OnProfilesNamesChanged;

    void OnProfilesNamesChanged(in NotifyCollectionChangedEventArgs<ProfileNameItemViewModel> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                {
                    Debug.Assert(item is { });
                    m_profileNameItemSubscriptions.Add(item, SubscribeProfileName(item));
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                {
                    Debug.Assert(item is { });
                    m_profileNameItemSubscriptions[item].Dispose();
                    m_profileNameItemSubscriptions.Remove(item);
                    m_profileNameEnable.OnNext((item.ProfileName, false));
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                foreach (var subscription in m_profileNameItemSubscriptions.Values)
                    subscription.Dispose();

                m_profileNameItemSubscriptions.Clear();

                foreach (var item in ProfilesNames)
                {
                    Debug.Assert(item is { });
                    m_profileNameItemSubscriptions.Add(item, SubscribeProfileName(item));
                }

                break;
        }
    }

    IDisposable SubscribeProfileName(ProfileNameItemViewModel item) =>
        item.WhenChanged(item => item.Enable)
            .Subscribe(enable => OnProfileEnable(item, enable));

    void OnProfileEnable(ProfileNameItemViewModel item, bool enable) =>
        m_profileNameEnable.OnNext((item.ProfileName, enable));

    bool CanAddProfileName() => TextBoxProfileName is { };

    [RelayCommand(CanExecute = nameof(CanAddProfileName))]
    public void AddProfileName()
    {
        Debug.Assert(TextBoxProfileName is { });
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
