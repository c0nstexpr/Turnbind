using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;

using MoreLinq;

using ObservableCollections;

using Turnbind.Action;
using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindsViewModel(Settings settings) : ObservableObject, IDisposable
{
    public readonly Settings Settings = settings;

    ProfileControlViewModel? m_profile;

    readonly SerialDisposable m_profileDisposable = new();

    public string? CurrentEditProfileName { get; private set; }

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

            var profilesNames = value.ProfilesNames;

            profilesNames.CollectionChanged += OnProfilesNamesChanged;
            m_profileDisposable.Disposable =
                Disposable.Create(() => profilesNames.CollectionChanged -= OnProfilesNamesChanged);
        }
    }

    readonly Dictionary<string, IDisposable> m_profileItemDisposable = [];

    void OnProfilesNamesChanged(in NotifyCollectionChangedEventArgs<ProfileNameItemViewModel> e)
    {
        var profiles = Settings.Profiles;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                {
                    profiles.Add(item.Name, []);

                    item.EditProfile.Subscribe(
                        _ =>
                        {
                            CurrentEditProfileName = item.Name;

                            if (KeyBindList is { })
                                KeyBindList.KeyBinds = new(
                                    profiles[item.Name].Select(
                                        pair =>
                                        new KeyValuePair<InputKeys, KeyBindViewModel>(
                                            pair.Key,
                                            new()
                                            {
                                                Keys = pair.Key,
                                                TurnSetting = new()
                                                {
                                                    Dir = pair.Value.Dir,
                                                    PixelPerSec = pair.Value.PixelPerSec
                                                }
                                            }
                                        )
                                    )
                                );
                        }
                    );

                }
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                    profiles.Remove(item.Name);
                break;

            case NotifyCollectionChangedAction.Reset:
                profiles.Clear();

                foreach (var item in m_profile!.ProfilesNames)
                    profiles.Add(item.Name, []);
                break;
        }

        Settings.Save();
    }

    KeyBindListViewModel? m_keyBindList;

    readonly SerialDisposable m_keyBindListDisposable = new();

    public KeyBindListViewModel? KeyBindList
    {
        get => m_keyBindList;

        set
        {
            m_keyBindList = value;

            if (value is null)
            {
                m_keyBindListDisposable.Disposable = null;
                return;
            }

            var keybind = value.KeyBinds;

            keybind.CollectionChanged += OnKeyBindsChanged;
            m_profileDisposable.Disposable =
                Disposable.Create(() => keybind.CollectionChanged -= OnKeyBindsChanged);
        }
    }

    void OnKeyBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, KeyBindViewModel>> e)
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
                    profiles.Add(item.Name, []);
                }

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
                    profiles.Add(item.Name, []);
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
