using System.Reactive;
using System.Reactive.Subjects;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Model;

namespace Turnbind.ViewModel;
partial class BindEditViewModel : ObservableObject
{
    KeyBindViewModel m_keyBind = new();

    public KeyBindViewModel KeyBind
    {
        get => m_keyBind;

        set => SetProperty(ref m_keyBind, value);
    }

    public void OnInputKey(InputKey k, bool p)
    {
        if (!p) return;

        //KeyBind.Keys.Add(k);
        //OnPropertyChanged(nameof(KeyString));
    }

    public void ClearKeys()
    {
        //KeyBind.Keys.Clear();
        //OnPropertyChanged(nameof(KeyString));
    }

    readonly Subject<Unit> m_add = new();

    readonly Subject<Unit> m_modify = new();

    readonly Subject<Unit> m_remove = new();

    public IObservable<Unit> Add => m_add;

    public IObservable<Unit> Modify => m_modify;

    public IObservable<Unit> Remove => m_remove;


    [RelayCommand(CanExecute =)]
    public void OnAdd() => m_add.OnNext(Unit.Default);

    public void OnModify() => throw new NotImplementedException();

    public void OnRemove() => throw new NotImplementedException();

    public void OnBindingKeysFocus() => throw new NotImplementedException();

    public void OnBindingKeysLostFocus() => throw new NotImplementedException();
}
