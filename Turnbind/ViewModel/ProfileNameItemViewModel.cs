using CommunityToolkit.Mvvm.ComponentModel;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject, IEquatable<ProfileNameItemViewModel>
{
    [ObservableProperty]
    string m_profileName = string.Empty;

    [ObservableProperty]
    bool m_enable = false;

    public bool Equals(ProfileNameItemViewModel? other) => ProfileName == other?.ProfileName;

    public override int GetHashCode() => ProfileName.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as ProfileNameItemViewModel);
}
