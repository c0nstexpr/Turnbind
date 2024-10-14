using System.Collections.Specialized;
using System.Reactive.Disposables;

using CommunityToolkit.Mvvm.ComponentModel;

using ObservableCollections;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class BindsViewModel : ObservableObject
{
    public required BindEditViewModel BindEdit { get; set; }

    readonly SerialDisposable m_keyBindsDisposble = new();

    ObservableDictionary<InputKeys, TurnSetting> m_keyBinds = [];

    public ObservableDictionary<InputKeys, TurnSetting> KeyBinds
    {
        get => m_keyBinds;

        set
        {
            SetProperty(ref m_keyBinds, value);

            BindEdit.ModifyCommand = new(
                () =>
                {
                    var turnSetting = BindEdit.TurnSetting;

                    value[new(BindEdit.InputKeys.Keys)] = new()
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
                () => value.Remove(BindEdit.InputKeys.Keys),
                () => value.ContainsKey(BindEdit.InputKeys.Keys)
            );

            value.CollectionChanged += OnBindsChanged;
            m_keyBindsDisposble.Disposable = Disposable.Create(() => value.CollectionChanged -= OnBindsChanged);
        }
    }

    void OnBindsChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<InputKeys, TurnSetting>> e)
    {
        if (e.Action is not NotifyCollectionChangedAction.Add or
            NotifyCollectionChangedAction.Remove or
            NotifyCollectionChangedAction.Reset) return;

        BindEdit.ModifyCommand.NotifyCanExecuteChanged();
    }

    KeyValuePair<InputKeys, TurnSetting> m_selected = new();

    public KeyValuePair<InputKeys, TurnSetting> Selected
    {
        get => m_selected;

        set
        {
            SetProperty(ref m_selected, value);

            BindEdit.InputKeys = new() { Keys = value.Key };
            BindEdit.TurnSetting = new() { TurnSetting = value.Value };
        }
    }

    public void Clear() => m_keyBinds.Clear();
}
