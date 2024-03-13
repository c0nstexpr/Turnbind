using System.Diagnostics;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MoreLinq;

using ObservableCollections;

using Turnbind.Action;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel(
    ProcessWindowAction processWindowAction,
    InputAction inputAction,
    TurnAction turnAction
) : ObservableObject, IDisposable
{
    readonly ProcessWindowAction m_processWindowAction = processWindowAction;

    readonly InputAction m_inputAction = inputAction;

    readonly TurnAction m_turnAction = turnAction;

    [ObservableProperty]
    ObservableHashSet<ProfileNameItemViewModel> m_profilesNames = [];

    readonly Dictionary<string, ProfileControl> m_profileControls = [];

    readonly Dictionary<string, CompositeDisposable> m_profileDisposables = [];

    public ProfileControl this[string name] => m_profileControls[name];

    [ObservableProperty]
    string? m_textBoxProfileName;

    bool CanAddProfileName() => TextBoxProfileName is { };

    [RelayCommand(CanExecute = nameof(CanAddProfileName))]
    void AddProfileName()
    {
        Debug.Assert(TextBoxProfileName is { });
        ProfileNameItemViewModel item = new() { ProfileName = TextBoxProfileName };

        if (!ProfilesNames.Add(item)) return;

        CompositeDisposable disposables = [];
        ProfileControl control = new(item.ProfileName);

        TextBoxProfileName = null;

        m_profileControls.Add(item.ProfileName, control);

        disposables.Add(
            item.WhenChanged(x => x.Enable).Subscribe(
                enable =>
                {
                    if (enable)
                        control.Enable(m_processWindowAction, m_inputAction, m_turnAction);
                    else control.Disable();
                }
            )
        );
        disposables.Add(item.RemoveProfile.Subscribe(_ => disposables.Dispose()));

        m_profileDisposables.Add(item.ProfileName, disposables);
    }

    public void Dispose()
    {
        m_profileControls.Values.ForEach(x => x.Dispose());
        m_profileDisposables.Values.ForEach(x => x.Dispose());
    }
}
