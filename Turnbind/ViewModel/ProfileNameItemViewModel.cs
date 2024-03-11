using System.Diagnostics;
using System.Reactive;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject, IEquatable<ProfileNameItemViewModel>
{
    public string ProfileName { init; get; } = string.Empty;

    [ObservableProperty]
    bool m_enable = false;

    public bool Equals(ProfileNameItemViewModel? other) => ProfileName == other?.ProfileName;

    public override int GetHashCode() => ProfileName.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as ProfileNameItemViewModel);

    readonly Subject<Unit> m_editProfile = new();

    public IObservable<Unit> EditProfile => m_editProfile;

    readonly Subject<Unit> m_removeProfile = new();

    public IObservable<Unit> RemoveProfile => m_editProfile;

    [RelayCommand]
    void OnRemoveProfile(ProfileNameItemViewModel item) => m_removeProfile.OnNext(Unit.Default);

    [RelayCommand]
    void OnEditProfile(ProfileNameItemViewModel item) => m_editProfile.OnNext(Unit.Default);
}
