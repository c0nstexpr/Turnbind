using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using ObservableCollections;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindsViewModel(Settings settings) : ObservableObject, IDisposable
{
    public readonly Settings Settings = settings;

    readonly SerialDisposable m_profileDisposable = new();

    ProfileControlViewModel? m_profile;

    public ProfileControlViewModel? Profile
    {
        get => m_profile;

        set
        {
            m_profile = value;

            if (value is null)
            {
                m_profileDisposable.Disposable = null;
                return;
            }

            CompositeDisposable disposables = [];

            {
                var profilesNames = value.ProfilesNames;
                profilesNames.CollectionChanged += OnProfilesNamesChanged;
                disposables.Add(Disposable.Create(() => value.ProfilesNames.CollectionChanged -= OnProfilesNamesChanged));
            }

            m_profileDisposable.Disposable = disposables;
        }
    }


    KeyBindListViewModel? m_keyBindList;

    public KeyBindListViewModel? KeyBindList
    {
        get => m_keyBindList;

        set
        {
            m_keyBindList = value;

            if (value is null)
            {
                return;
            }
        }
    }

    void EnableProfile((string, bool) tuple)
    {
        var (profile, enable) = tuple;

        if (!enable)
        {
            return;
        }

        var keyBinds = Settings.Profiles[profile];
    }

    readonly SerialDisposable m_keyBindListDisposable = new();

    void OnProfilesNamesChanged(in NotifyCollectionChangedEventArgs<ProfileNameItemViewModel> e)
    {
        var profiles = Settings.Profiles;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                    profiles.Add(item.ProfileName, []);
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                    profiles.Remove(item.ProfileName);
                break;
            case NotifyCollectionChangedAction.Replace:

                break;

            case NotifyCollectionChangedAction.Reset:
                profiles.Clear();

                foreach (var item in m_profile!.ProfilesNames)
                    profiles.Add(item.ProfileName, []);
                break;
        }

        Settings.Save();
    }

    void OnKeyBindChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, KeyBindViewModel>> e)
    {
        var profiles = Settings.Profiles[];

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var (keys, keyBind) in e.NewItems)
                {
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                {
                    Debug.Assert(item is { });
                    profiles.Remove(item.ProfileName);
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                profiles.Clear();

                foreach (var item in m_profile!.ProfilesNames)
                {
                    Debug.Assert(item is { });
                    profiles.Add(item.ProfileName, []);
                }

                break;
        }

        Settings.Save();
    }

    public void Dispose()
    {
        m_profileDisposable.Dispose();
    }
}
