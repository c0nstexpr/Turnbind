using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ObservableCollections;

using SpanLinq;

using Turnbind.Helper;
using Turnbind.Model;

namespace Turnbind.ViewModel;

sealed partial class BindsViewModel : ObservableObject, IDisposable
{
    public ObservableDictionary<InputKeys, TurnSetting> TurnBindsDic { get; } = [];

    readonly IDisposable m_disposable;

    readonly ISynchronizedView<KeyValuePair<InputKeys, TurnSetting>, BindsItemViewModel> m_itemsView;

    readonly INotifyCollectionChangedSynchronizedViewList<BindsItemViewModel> m_itemSource;

    public INotifyCollectionChanged ItemSource => m_itemSource;

    BindsItemViewModel m_selected = new();

    public BindsItemViewModel Selected
    {
        get => m_selected;

        set
        {
            SetProperty(ref m_selected, value);
            value.WhenChanged(x => x.InputKeys).Subscribe(
                _ => RemoveCommand.NotifyCanExecuteChanged()
            );
        }
    }

    public BindsViewModel()
    {
        TurnBindsDic.CollectionChanged += OnBindsChanged;

        m_itemsView = TurnBindsDic.CreateCollectionView(
            p => new BindsItemViewModel
            {
                InputKeys = new() { Keys = p.Key },
                TurnSetting = new() { TurnSetting = p.Value }
            }
        );
        m_itemSource = m_itemsView.ToNotifyCollectionChanged();

        m_disposable = new CompositeDisposable(m_itemSource, m_itemsView);
    }

    bool CanModify() => !Selected.InputKeys.Keys.Empty;

    [RelayCommand(CanExecute = nameof(CanModify))]
    void Modify()
    {
        var keys = Selected.InputKeys;
        var turnSetting = Selected.TurnSetting;

        TurnBindsDic[new(keys.Keys)] = new()
        {
            Dir = turnSetting.Dir,
            PixelPerMs = turnSetting.PixelPerMs,
            WheelFactor = turnSetting.WheelFactor
        };

        RemoveCommand.NotifyCanExecuteChanged();
    }

    bool CanRemove() => TurnBindsDic.ContainsKey(Selected.InputKeys.Keys);

    [RelayCommand(CanExecute = nameof(CanRemove))]
    void Remove() => TurnBindsDic.Remove(Selected.InputKeys.Keys);

    void OnBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, TurnSetting>> e)
    {
        var action = e.Action;
        var cmd = RemoveCommand;

        if (action == NotifyCollectionChangedAction.Reset)
        {
            cmd.NotifyCanExecuteChanged();
            return;
        }

        var keys = Selected.InputKeys.Keys;

        if (e.IsSingleItem)
        {
            var newKeys = action switch
            {
                NotifyCollectionChangedAction.Add => e.NewItem,
                NotifyCollectionChangedAction.Remove => e.OldItem,
                _ => new(),
            };

            if (keys.Equals(newKeys.Key)) cmd.NotifyCanExecuteChanged();

            return;
        }

        var newKeysSpan = action switch
        {
            NotifyCollectionChangedAction.Add => e.NewItems,
            NotifyCollectionChangedAction.Remove => e.OldItems,
            _ => null,
        };

        if(newKeysSpan.Any(newKeys => keys.Equals(newKeys.Key)))
            cmd.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        TurnBindsDic.CollectionChanged -= OnBindsChanged;
        m_disposable.Dispose();
    }
}
