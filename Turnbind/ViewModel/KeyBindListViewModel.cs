using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using ObservableCollections;

using Turnbind.Helper;
using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindListViewModel : ObservableObject
{
    readonly KeyBindEditViewModel m_keyBindEdit;

    public required KeyBindEditViewModel KeyBindEdit
    {
        get => m_keyBindEdit;

        [MemberNotNull(nameof(m_keyBindEdit))]
        init
        {
            m_keyBindEdit = value;

            value.AddCommand = new(Add, CanAdd);
            value.RemoveCommand = new(Remove, CanRemove);
            value.ModifyCommand = new(Modify, CanRemove);
        }
    }

    readonly ObservableDictionary<InputKeys, KeyBindViewModel> m_keyBinds = [];

    internal IObservableCollection<KeyValuePair<InputKeys, KeyBindViewModel>> m_observableKeyBinds => m_keyBinds;

    readonly ObservableDicListView<InputKeys, KeyBindViewModel> m_keyBindsListView;

    public ObservableDicValueListView<InputKeys, KeyBindViewModel> KeyBinds { get; }

    KeyBindViewModel? m_selected;

    public KeyBindViewModel? Selected
    {
        get => m_selected;

        set
        {
            SetProperty(ref m_selected, value);

            if (value is null) return;

            var turnSetting = value.TurnSetting;

            KeyBindEdit.KeyBind = new()
            {
                Keys = new(value.Keys),
                Dir = turnSetting.Dir,
                PixelPerMs = turnSetting.PixelPerMs
            };
        }
    }

    public KeyBindListViewModel()
    {
        m_keyBindsListView = new(m_keyBinds);
        KeyBinds = m_keyBindsListView.CreateValueView();
    }

    public KeyBindViewModel? Add(InputKeys keys, TurnSetting turnSetting)
    {
        KeyBindViewModel vm = new()
        {
            Keys = keys,
            Dir = turnSetting.Dir,
            PixelPerMs = turnSetting.PixelPerMs,
        };

        var res = m_keyBinds.TryAdd(keys, vm) ? vm : null;

        KeyBindEdit.AddCommand.NotifyCanExecuteChanged();
        KeyBindEdit.ModifyCommand.NotifyCanExecuteChanged();
        KeyBindEdit.RemoveCommand.NotifyCanExecuteChanged();

        return res;
    }

    public void Remove(InputKeys keys)
    {
        m_keyBinds.Remove(keys);

        KeyBindEdit.AddCommand.NotifyCanExecuteChanged();
        KeyBindEdit.ModifyCommand.NotifyCanExecuteChanged();
        KeyBindEdit.RemoveCommand.NotifyCanExecuteChanged();
    }

    public void Clear() => m_keyBinds.Clear();

    void Add()
    {
        var keyBind = KeyBindEdit.KeyBind;
        Add(keyBind.Keys, keyBind.TurnSetting);
    }

    bool CanAdd()
    {
        var keys = KeyBindEdit.KeyBind.Keys;
        return keys.Count > 0 && !m_keyBinds.ContainsKey(keys);
    }

    void Remove() => Remove(Selected!.Keys);

    bool CanRemove() => Selected?.Keys.Equals(KeyBindEdit.KeyBind.Keys) == true;

    void Modify()
    {
        var keyBind = m_keyBinds[Selected!.Keys];
        var turnSetting = KeyBindEdit.KeyBind.TurnSetting;

        m_keyBinds[Selected!.Keys] = new()
        {
            Keys = keyBind.Keys,
            Dir = turnSetting.Dir,
            PixelPerMs = turnSetting.PixelPerMs
        };

        KeyBindEdit.AddCommand.NotifyCanExecuteChanged();
        KeyBindEdit.ModifyCommand.NotifyCanExecuteChanged();
        KeyBindEdit.RemoveCommand.NotifyCanExecuteChanged();
    }
}
