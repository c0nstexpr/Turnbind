using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Helper;

namespace Turnbind.ViewModel;

partial class ProfileNameItemViewModel : ObservableObject, IDisposable
{
    public string Name { init; get; } = string.Empty;

    readonly Subject<Unit> m_editProfile = new();

    public IObservable<Unit> EditProfile => m_editProfile;

    readonly Subject<Unit> m_removeProfile = new();

    public IObservable<Unit> RemoveProfile => m_removeProfile;

    readonly BehaviorSubject<bool> m_enableProfile = new(false);

    public BehaviorObservable<bool> EnableProfile { get; }

    string m_toggleContent = "OFF";

    public string ToggleContent
    {
        get => m_toggleContent;

        private set => SetProperty(ref m_toggleContent, value);
    }

    public ProfileNameItemViewModel() => EnableProfile = m_enableProfile.AsObservable();

    [RelayCommand]
    void OnEnableProfile(bool enable)
    {
        m_enableProfile.OnNext(enable);
        ToggleContent = enable ? "ON" : "OFF";
    }

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
