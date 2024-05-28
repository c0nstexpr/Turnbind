using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
    readonly ILogger<KeyBindsViewModel> m_logger = App.GetRequiredService<ILogger<KeyBindsViewModel>>();

    readonly Settings m_settings = App.GetRequiredService<Settings>();

    readonly Dictionary<string, ProfileControl> m_profileControls = [];

    readonly CompositeDisposable m_disposable = [];

    readonly ProfileControlViewModel m_profile;

    public required ProfileControlViewModel Profile
    {
        get => m_profile;

        [MemberNotNull(nameof(m_profile))]
        init
        {
            m_profile = value;

            var profilesNames = value.m_observableProfilesNames;

            foreach (var name in m_settings.Profiles.Keys)
                SubscribeProfile(Profile.Add(name)!);

            profilesNames.CollectionChanged += OnProfilesNamesChanged;

            m_disposable.Add(() => profilesNames.CollectionChanged -= OnProfilesNamesChanged);
        }
    }

    readonly KeyBindListViewModel m_keyBindList;

    public required KeyBindListViewModel KeyBindList
    {
        get => m_keyBindList;

        [MemberNotNull(nameof(m_keyBindList))]
        init
        {
            m_keyBindList = value;

            var keyBinds = value.m_observableKeyBinds;

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

            m_modifyingKeyBinds = true;

            KeyBindList.Clear();

            if (value is null)
            {
                KeyBindListEnable = false;
                return;
            }

            KeyBindListEnable = true;

            foreach (var (keys, turnsetting) in m_settings.Profiles[value])
                KeyBindList.Add(keys, turnsetting);

            m_modifyingKeyBinds = false;

            OnPropertyChanged(nameof(ProfileTitle));
        }
    }

    public string ProfileTitle => $"Current Profile: {CurrentEditProfileName}";

    bool m_keyBindListEnable;

    public bool KeyBindListEnable
    {
        get => m_keyBindListEnable;

        private set => SetProperty(ref m_keyBindListEnable, value);
    }

    readonly Dictionary<string, IDisposable> m_profileDiposables = [];

    void OnProfilesNamesChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<string, ProfileNameItemViewModel>> e)
    {
        var profiles = m_settings.Profiles;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var (_, item) in e.NewItems)
                    OnProfileAdd(item);

                OnProfileAdd(e.NewItem.Value);

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var (_, item) in e.OldItems)
                    OnProfileRemove(item);

                OnProfileRemove(e.OldItem.Value);

                break;

            case NotifyCollectionChangedAction.Reset:
                profiles.Clear();
                m_profileControls.Values.ForEach(x => x.Dispose());
                m_profileControls.Clear();

                foreach (var (name, _) in Profile.m_observableProfilesNames)
                    profiles.Add(name, []);

                m_logger.LogInformation("Reset profiles");

                break;
        }

        m_settings.Save();
    }

    void OnProfileRemove(ProfileNameItemViewModel item)
    {
        var profiles = m_settings.Profiles;
        var name = item.Name;

        profiles.Remove(name);

        m_profileDiposables[name].Dispose();
        m_profileDiposables.Remove(name);

        if (m_profileControls.TryGetValue(name, out var control))
        {
            control.Dispose();
            m_profileControls.Remove(name);
        }

        if (CurrentEditProfileName == name) CurrentEditProfileName = null;

        m_logger.LogInformation("Remove profile {ProfileName}", name);
    }

    void OnProfileAdd(ProfileNameItemViewModel item)
    {
        var profiles = m_settings.Profiles;
        var name = item.Name;

        profiles.Add(name, []);
        SubscribeProfile(item);

        m_logger.LogInformation("Add profile {ProfileName}", name);
    }

    void SubscribeProfile(ProfileNameItemViewModel item)
    {
        var name = item.Name;

        m_profileDiposables[name] = new CompositeDisposable
        {
            item.EditProfile.Subscribe(_ => CurrentEditProfileName = name),
            item.EnableProfile.Subscribe(
                enable =>
                {
                    if(!m_profileControls.TryGetValue(name, out var control))
                    {
                        control = new(name);
                        m_profileControls[name] = control;
                        Initialize(control);
                    }

                    control.Enable = enable;
                }
            )
        };
    }

    void Initialize(ProfileControl control)
    {
        foreach (var (keys, turnsetting) in m_settings.Profiles[control.ProfileName])
            control.Add(keys, turnsetting);
    }

    bool m_modifyingKeyBinds = false;

    void OnKeyBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, KeyBindViewModel>> e)
    {
        if (m_modifyingKeyBinds || CurrentEditProfileName is null) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var pair in e.NewItems)
                    OnKeyBindAdd(pair);

                OnKeyBindAdd(e.NewItem);

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var pair in e.OldItems)
                    OnKeyBindRemove(pair);

                OnKeyBindRemove(e.OldItem);

                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (var pair in e.NewItems)
                    OnKeyBindReplace(pair);

                OnKeyBindReplace(e.NewItem);

                break;

            case NotifyCollectionChangedAction.Reset:
                var profileName = CurrentEditProfileName!;
                var keybinds = m_settings.Profiles[profileName];

                if (!m_profileControls.TryGetValue(profileName, out var control))
                {
                    control = new(profileName);
                    m_profileControls[profileName] = control;
                }

                keybinds.Clear();
                control.Clear();

                foreach (var (keys, turnsetting) in KeyBindList.m_observableKeyBinds)
                {
                    var setting = turnsetting.TurnSetting;
                    keybinds.Add(keys, setting);
                    control.Add(keys, setting);
                }

                m_logger.LogInformation("Reset keybinds in profile {ProfileName}", profileName);

                break;
        }

        m_settings.Save();
    }

    void OnKeyBindAdd(KeyValuePair<InputKeys, KeyBindViewModel> pair)
    {
        var (keys, keyBind) = pair;
        var profileName = CurrentEditProfileName!;

        if (!m_profileControls.TryGetValue(profileName, out var control))
        {
            control = new(profileName);
            m_profileControls[profileName] = control;
            Initialize(control);
        }

        var setting = keyBind.TurnSetting;

        m_settings.Profiles[profileName].Add(
            keys,
            new()
            {
                Dir = setting.Dir,
                PixelPerMs = setting.PixelPerMs
            }
        );

        control.Add(keys, setting);

        m_logger.LogInformation(
            "Add keybind {Keys} to profile {ProfileName}",
            keyBind.KeysString,
            profileName
        );
    }

    void OnKeyBindRemove(KeyValuePair<InputKeys, KeyBindViewModel> pair)
    {
        var (keys, keyBind) = pair;
        var profileName = CurrentEditProfileName!;

        m_settings.Profiles[profileName].Remove(keys);

        {
            if (m_profileControls.TryGetValue(profileName, out var control))
                control.Remove(keys);
        }

        m_logger.LogInformation(
            "Remove keybind {KeysString} from profile {ProfileName}",
            keyBind.KeysString,
            profileName
        );
    }

    void OnKeyBindReplace(KeyValuePair<InputKeys, KeyBindViewModel> pair)
    {
        var (keys, keyBind) = pair;
        var profileName = CurrentEditProfileName!;
        var turnSetting = keyBind.TurnSetting;

        {
            if (m_profileControls.TryGetValue(profileName, out var control))
                control.Update(keys, turnSetting);
        }

        m_settings.Profiles[profileName][keys] = turnSetting;

        m_logger.LogInformation("Update keybind {KeysString} in profile {ProfileName}", keyBind.KeysString, profileName);
    }

    public void Dispose()
    {
        m_profileControls.Values.ForEach(x => x.Dispose());
        m_disposable.Dispose();
    }
}
