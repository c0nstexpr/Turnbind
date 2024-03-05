using System.Reactive;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using Turnbind.Model;

namespace Turnbind.ViewModel;
partial class TurnBindEditViewModel : ObservableObject
{
    KeyBindViewModel m_keyBind = new();

    public KeyBindViewModel KeyBind
    {
        get => m_keyBind;

        set => SetProperty(ref m_keyBind, value);
    }

    [ObservableProperty]
    bool m_addButtonEnabled;

    [ObservableProperty]
    bool m_modifyButtonEnabled;

    [ObservableProperty]
    bool m_removeButtonEnabled;

    public void OnInputKey(InputKey k, bool p)
    {
        if (!p) return;

        KeyBind.Keys.Add(k);
        OnPropertyChanged(nameof(KeyString));
    }

    public void ClearKeys()
    {
        KeyBind.Keys.Clear();
        OnPropertyChanged(nameof(KeyString));
    }

    readonly Subject<Unit> m_add = new();

    readonly Subject<Unit> m_modify = new();

    readonly Subject<Unit> m_remove = new();

    public void OnAdd() => m_add.OnNext(

    public void OnModify() => throw new NotImplementedException();

    public void OnRemove() => throw new NotImplementedException();



    public void OnBindingKeysFocus() => throw new NotImplementedException();

    public void OnBindingKeysLostFocus() => throw new NotImplementedException();
}
