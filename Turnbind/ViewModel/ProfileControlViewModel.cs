using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ObservableCollections;

using Turnbind.Helper;

namespace Turnbind.ViewModel;

partial class ProfileControlViewModel : ObservableObject, IDisposable
{
    readonly IDisposable m_disposable;

    public ObservableHashSet<string> Profiles { get; } = [];

    readonly ISynchronizedView<string, ProfileItemViewModel> m_profilesView;

    readonly INotifyCollectionChangedSynchronizedViewList<ProfileItemViewModel> m_itemSource;

    public INotifyCollectionChanged ItemSource => m_itemSource;

    string? m_inputBoxProfileName;

    public string? InputProfile
    {
        get => m_inputBoxProfileName;

        set
        {
            SetProperty(ref m_inputBoxProfileName, value);
            AddProfileCommand.NotifyCanExecuteChanged();
        }
    }

    readonly BehaviorSubject<string?> m_viewedItem = new(null);

    public IObservable<string?> ViewedItem => m_viewedItem;

    public ProfileControlViewModel()
    {
        Profiles.CollectionChanged += OnProfilesChanged;
        m_profilesView = Profiles.CreateCollectionView(
            (string n) => new ProfileItemViewModel()
            {
                Name = n,
                ViewCmd = new(() => m_viewedItem.OnNext(n)),
                RemoveCmd = new(() => Profiles.Remove(n))
            }
        );
        m_itemSource = m_profilesView.ToNotifyCollectionChanged();
        m_disposable = new CompositeDisposable(m_itemSource, m_profilesView, m_viewedItem);
    }

    void OnProfilesChanged(in NotifyCollectionChangedEventArgs<string> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                m_viewedItem.OnNext(null);
                break;

            case NotifyCollectionChangedAction.Remove && e.IsSingleItem:
                if (e.IsSingleItem)
                {

                }

                break;


        }
    }

    bool CanAddProfile() => InputProfile is { } n && !Profiles.Contains(n);

    [RelayCommand(CanExecute = nameof(CanAddProfile))]
    void AddProfile()
    {
        Profiles.Add(InputProfile!);
        InputProfile = null;
    }

    public void Dispose() => m_disposable.Dispose();
}
