using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;

using ObservableCollections;

using Turnbind.Helper;
using Turnbind.Model;

namespace Turnbind.ViewModel;

sealed partial class BindsViewModel : ObservableObject, IDisposable
{
    BindEditViewModel m_bindEdit;

    public required BindEditViewModel BindEdit
    {
        get => m_bindEdit;

        [MemberNotNull(nameof(m_bindEdit))]
        set
        {
            m_bindEdit = value;
            UpdateEdit();
        }
    }

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
            UpdateEdit();
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

    void UpdateEdit()
    {
        BindEdit.InputKeys = new() { Keys = Selected.InputKeys.Keys };
        BindEdit.TurnSetting = new()
        {
            Dir = Selected.TurnSetting.Dir,
            PixelPerMs = Selected.TurnSetting.PixelPerMs,
            WheelFactor = Selected.TurnSetting.WheelFactor
        };

        BindEdit.ModifyCommand = new(
            () =>
            {
                var keys = BindEdit.InputKeys;
                var turnSetting = BindEdit.TurnSetting;

                InputKeys newKeys = new(keys.Keys);

                TurnBindsDic[newKeys] = new()
                {
                    Dir = turnSetting.Dir,
                    PixelPerMs = turnSetting.PixelPerMs,
                    WheelFactor = turnSetting.WheelFactor
                };

                BindEdit.RemoveCommand.NotifyCanExecuteChanged();
            },
            () => !BindEdit.InputKeys.Keys.Empty
        );

        BindEdit.RemoveCommand = new(
            () => TurnBindsDic.Remove(BindEdit.InputKeys.Keys),
            () => TurnBindsDic.ContainsKey(BindEdit.InputKeys.Keys)
        );
    }

    void OnBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, TurnSetting>> e)
    {
        var action = e.Action;
        var cmd = BindEdit.RemoveCommand;

        if (action == NotifyCollectionChangedAction.Reset)
        {
            cmd.NotifyCanExecuteChanged();
            return;
        }

        var keys = BindEdit.InputKeys.Keys;

        if (e.IsSingleItem)
        {
            var newKeys = action switch
            {
                NotifyCollectionChangedAction.Add => e.NewItem.Key,
                NotifyCollectionChangedAction.Remove => e.OldItem.Key,
                _ => null,
            };

            if (keys.Equals(newKeys)) cmd.NotifyCanExecuteChanged();
        }
        else
        {
            var newKeysSpan = action switch
            {
                NotifyCollectionChangedAction.Add => e.NewItems,
                NotifyCollectionChangedAction.Remove => e.OldItems,
                _ => null,
            };

            foreach (var (newKeys, _) in newKeysSpan)
                if (keys.Equals(newKeys))
                {
                    cmd.NotifyCanExecuteChanged();
                    break;
                }
        }
    }

    public void Dispose()
    {
        TurnBindsDic.CollectionChanged -= OnBindsChanged;
        m_disposable.Dispose();
    }
}
