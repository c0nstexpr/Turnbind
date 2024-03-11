using CommunityToolkit.Mvvm.ComponentModel;

using ObservableCollections;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindListViewModel : ObservableObject
{
    KeyBindEditViewModel? m_keyBindEdit;

    public KeyBindEditViewModel? KeyBindEdit
    {
        get => m_keyBindEdit;

        set
        {
            m_keyBindEdit = value;

            if (value is null) return;

            value.AddCommand = new(Add, CanAdd);
            value.RemoveCommand = new(Remove, CanRemove);
            value.ModifyCommand = new(Modify, CanRemove);
        }
    }

    [ObservableProperty]
    ObservableDictionary<InputKeys, KeyBindViewModel> m_keyBinds = [];

    KeyBindViewModel? m_selected;

    public KeyBindViewModel? Selected
    {
        get => m_selected;

        set
        {
            SetProperty(ref m_selected, value);

            if (value is null || KeyBindEdit is null) return;

            var turnSetting = value.TurnSetting;
            KeyBindEdit.KeyBind = new()
            {
                Keys = new(value.Keys),
                TurnSetting = new()
                {
                    Dir = turnSetting.Dir,
                    PixelPerSec = turnSetting.PixelPerSec,
                }
            };
        }
    }

    void Add()
    {
        var keyBind = KeyBindEdit!.KeyBind;
        InputKeys keys = new(keyBind.Keys);
        var turnSetting = keyBind.TurnSetting;

        KeyBinds.Add(
            keys,
            new()
            {
                Keys = keys,
                TurnSetting = new()
                {
                    Dir = turnSetting.Dir,
                    PixelPerSec = turnSetting.PixelPerSec,
                }
            }
        );

        Selected = keyBind;
    }

    bool CanAdd() => !KeyBinds.ContainsKey(KeyBindEdit!.KeyBind.Keys);

    void Remove() => KeyBinds.Remove(Selected!.Keys);

    bool CanRemove() => Selected is { };

    void Modify()
    {
        var keyBind = KeyBinds[Selected!.Keys];
        var turnSetting = KeyBindEdit!.KeyBind.TurnSetting;

        KeyBinds[Selected!.Keys] = new()
        {
            Keys = keyBind.Keys,
            TurnSetting = new()
            {
                Dir = turnSetting.Dir,
                PixelPerSec = turnSetting.PixelPerSec,
            }
        };
    }
}
