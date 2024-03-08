using CommunityToolkit.Mvvm.ComponentModel;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject, IEquatable<ProfileNameItemViewModel>
{
    public string ProfileName { init; get; } = string.Empty;

    [ObservableProperty]
    bool m_enable = false;

    public bool Equals(ProfileNameItemViewModel? other) => ProfileName == other?.ProfileName;

    public override int GetHashCode() => ProfileName.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as ProfileNameItemViewModel);
}
