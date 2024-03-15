using System.Reactive;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject, IEquatable<ProfileNameItemViewModel>, IDisposable
{
    public string Name { init; get; } = string.Empty;

    [ObservableProperty]
    bool m_enable = false;

    public bool Equals(ProfileNameItemViewModel? other) => Name == other?.Name;

    public override int GetHashCode() => Name.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as ProfileNameItemViewModel);

    readonly Subject<Unit> m_editProfile = new();

    public IObservable<Unit> EditProfile => m_editProfile;

    readonly Subject<Unit> m_removeProfile = new();

    public IObservable<Unit> RemoveProfile => m_editProfile;

    readonly Subject<Unit> m_enableProfile = new();

    public IObservable<Unit> EnableProfile => m_enableProfile;

    [RelayCommand]
    void OnEnableProfile() => m_enableProfile.OnNext(default);

    [RelayCommand]
    void OnRemoveProfile() => m_removeProfile.OnNext(default);

    [RelayCommand]
    void OnEditProfile() => m_editProfile.OnNext(default);

    public void Dispose()
    {
        m_editProfile.Dispose();
        m_removeProfile.Dispose();
        m_enableProfile.Dispose();
    }
}
