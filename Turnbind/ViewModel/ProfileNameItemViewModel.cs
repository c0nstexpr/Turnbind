using System.Reactive;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Action;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject, IEquatable<ProfileNameItemViewModel>, IDisposable
{
    public string Name { init; get; } = string.Empty;

    public readonly ProfileControl Control;

    public ProfileNameItemViewModel() => Control = new(Name);

    [ObservableProperty]
    bool m_enable = false;

    public bool Equals(ProfileNameItemViewModel? other) => Name == other?.Name;

    public override int GetHashCode() => Name.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as ProfileNameItemViewModel);

    readonly Subject<Unit> m_editProfile = new();

    public IObservable<Unit> EditProfile => m_editProfile;

    readonly Subject<Unit> m_removeProfile = new();

    public IObservable<Unit> RemoveProfile => m_editProfile;

    [RelayCommand]
    void OnEnableProfile()
    {
        if (Enable) Control.Enable();
        else Control.Disable();
    }

    [RelayCommand]
    void OnRemoveProfile()
    {
        m_removeProfile.OnNext(Unit.Default);
        Dispose();
    }

    [RelayCommand]
    void OnEditProfile() => m_editProfile.OnNext(Unit.Default);

    public void Dispose()
    {
        Control.Dispose();
        m_editProfile.Dispose();
        m_removeProfile.Dispose();
    }
}
