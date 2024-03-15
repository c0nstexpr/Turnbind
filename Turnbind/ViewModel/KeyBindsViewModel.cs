using System.Collections.Specialized;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Logging;

using MoreLinq;

using ObservableCollections;

using Turnbind.Action;
using Turnbind.Helper;
using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindsViewModel : ObservableObject, IDisposable
{
    readonly ILogger<KeyBindsViewModel> m_logger = App.GetService<ILogger<KeyBindsViewModel>>();

    readonly Settings m_settings = App.GetService<Settings>();

    readonly Dictionary<string, ProfileControl> m_profileControls = [];

    readonly CompositeDisposable m_disposable = [];

#pragma warning disable CS8618

    readonly ProfileControlViewModel m_profile;

    public required ProfileControlViewModel Profile
    {
        get => m_profile;
        
        init
        {
            m_profile = value;

            var profilesNames = value.ProfilesNames;

            profilesNames.CollectionChanged += OnProfilesNamesChanged;

            m_disposable.Add(() => profilesNames.CollectionChanged -= OnProfilesNamesChanged);
        }
    }

    readonly KeyBindListViewModel m_keyBindList;

#pragma warning restore CS8618 

    public KeyBindListViewModel KeyBindList
    {
        get => m_keyBindList; init
        {
            m_keyBindList = value;

            var keyBinds = value.KeyBinds;

            keyBinds.CollectionChanged += OnKeyBindsChanged;

            m_disposable.Add(() => keyBinds.CollectionChanged -= OnKeyBindsChanged);
        }
    }

    string? m_currentEditProfileName;

    public string? CurrentEditProfileName
    {
        get => m_currentEditProfileName;

        private set
        {
            m_currentEditProfileName = value;

            var keyBinds = KeyBindList.KeyBinds;

            m_modifyingKeyBinds = true;

            if (value is null)
            {
                keyBinds.Clear();
                return;
            }

            var profiles = m_settings.Profiles;

            keyBinds.Clear();

            foreach (var (keys, turnsetting) in profiles[value])
                keyBinds.Add(
                    keys,
                    new()
                    {
                        Keys = new(keys),
                        TurnSetting = new()
                        {
                            Dir = turnsetting.Dir,
                            PixelPerSec = turnsetting.PixelPerSec,
                        }
                    }
                );

            m_modifyingKeyBinds = false;
        }
    }

    readonly Dictionary<string, IDisposable> m_profileDiposables = [];

    void OnProfilesNamesChanged(in NotifyCollectionChangedEventArgs<ProfileNameItemViewModel> e)
    {
        var profiles = m_settings.Profiles;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                {
                    profiles.Add(item.Name, []);
                    m_profileDiposables[item.Name] =
                        item.EditProfile.Subscribe(_ => CurrentEditProfileName = item.Name);

                    m_logger.LogInformation("Add profile {ProfileName}", item.Name);
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                {
                    profiles.Remove(item.Name);

                    m_profileDiposables[item.Name].Dispose();
                    m_profileDiposables.Remove(item.Name);

                    if (m_profileControls.TryGetValue(item.Name, out var control))
                    {
                        control.Dispose();
                        m_profileControls.Remove(item.Name);
                    }

                    m_logger.LogInformation("Remove profile {ProfileName}", item.Name);
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                profiles.Clear();
                m_profileControls.Values.ForEach(x => x.Dispose());
                m_profileControls.Clear();

                foreach (var item in Profile.ProfilesNames)
                    profiles.Add(item.Name, []);

                m_logger.LogInformation("Reset profiles");

                break;
        }

        m_settings.Save();
    }

    bool m_modifyingKeyBinds = false;

    void OnKeyBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, KeyBindViewModel>> e)
    {
        if (m_modifyingKeyBinds) return;

        var profileName = CurrentEditProfileName!;
        var keybinds = m_settings.Profiles[profileName];

        if (!m_profileControls.TryGetValue(profileName, out var control))
        {
            control = new(profileName);
            m_profileControls[profileName] = control;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var (keys, keyBind) in e.NewItems)
                {
                    var setting = keyBind.TurnSetting.Setting;

                    keybinds.Add(
                        keys,
                        new()
                        {
                            Dir = setting.Dir,
                            PixelPerSec = setting.PixelPerSec
                        }
                    );

                    control.Add(keys, setting);

                    m_logger.LogInformation(
                        "Add keybind {Keys} to profile {ProfileName}",
                        keyBind.KeysString,
                        profileName
                    );
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var (keys, keyBind) in e.OldItems)
                {
                    keybinds.Remove(keys);
                    control.Remove(keys);

                    m_logger.LogInformation(
                        "Remove keybind {KeysString} from profile {ProfileName}",
                        keyBind.KeysString,
                        profileName
                    );
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (var (keys, keyBind) in e.NewItems)
                {
                    var turnSetting = keyBind.TurnSetting.Setting;

                    keybinds[keys] = turnSetting;
                    control.Update(keys, turnSetting);

                    m_logger.LogInformation("Update keybind {KeysString} in profile {ProfileName}", keyBind.KeysString, profileName);
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                keybinds.Clear();
                control.Clear();

                m_logger.LogInformation("Reset keybinds in profile {ProfileName}", profileName);

                break;
        }

        m_settings.Save();
    }

    public void Dispose()
    {
        m_profileControls.Values.ForEach(x => x.Dispose());
        m_disposable.Dispose();
    }
}
