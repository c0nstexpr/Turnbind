using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using LanguageExt;

using ObservableCollections;

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

    public readonly ObservableDictionary<InputKeys, KeyBindViewModel> KeyBinds = [];

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
        var keyBind = KeyBindEdit.KeyBind;
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

    bool CanAdd()
    {
        var keys = KeyBindEdit.KeyBind.Keys;
        return keys.Count > 0 && !KeyBinds.ContainsKey(keys);
    }

    void Remove() => KeyBinds.Remove(Selected!.Keys);

    bool CanRemove() => Selected is { };

    void Modify()
    {
        var keyBind = KeyBinds[Selected!.Keys];
        var turnSetting = KeyBindEdit.KeyBind.TurnSetting;

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
