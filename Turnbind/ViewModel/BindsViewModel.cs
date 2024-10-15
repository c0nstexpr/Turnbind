using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;

using LanguageExt.ClassInstances.Pred;

using ObservableCollections;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class BindsViewModel : ObservableObject
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

    public ObservableDictionary<InputKeys, BindsItemViewModel> BindsDic { get; } = [];

    public INotifyCollectionChangedSynchronizedViewList<BindsItemViewModel> Items { get; }

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
        BindsDic.CollectionChanged += OnBindsChanged;
        Items = BindsDic.ToNotifyCollectionChanged(p => p.Value);
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

                BindsDic[newKeys] = new()
                {
                    InputKeys = new() { Keys = newKeys },
                    TurnSetting = new()
                    {
                        Dir = turnSetting.Dir,
                        PixelPerMs = turnSetting.PixelPerMs,
                        WheelFactor = turnSetting.WheelFactor
                    }
                };

                BindEdit.RemoveCommand.NotifyCanExecuteChanged();
            },
            () => !BindEdit.InputKeys.Keys.Empty
        );

        BindEdit.RemoveCommand = new(
            () => BindsDic.Remove(BindEdit.InputKeys.Keys),
            () => BindsDic.ContainsKey(BindEdit.InputKeys.Keys)
        );
    }

    void OnBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, BindsItemViewModel>> e)
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

    public void Clear() => BindsDic.Clear();
}
