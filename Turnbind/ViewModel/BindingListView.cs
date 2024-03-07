using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;
internal class BindingListView : ObservableObject
{
    public ObservableCollection<KeyBindViewModel> Binds { get; } = [];

    int m_focusedBindingIndex = -1;

    public int FocusedBindingIndex
    {
        get => m_focusedBindingIndex;

        set
        {
            SetProperty(ref m_focusedBindingIndex, value);
            OnPropertyChanged(nameof(m_focusedBinding));

            if (m_focusedBinding is null) return;
        }
    }

    public void SetBinds(HashSet<TurnSetting> binds)
    {
        Binds.Clear();

        foreach (var bind in binds)
            Binds.Add(
                new()
                {
                    //Dir = bind.Dir,
                    //PixelPerSec = bind.PixelPerSec,
                    //Keys = bind.Keys
                }
            );
    }

    KeyBindViewModel? m_focusedBinding => FocusedBindingIndex >= 0 && FocusedBindingIndex < Binds.Count ?
        Binds[FocusedBindingIndex] : null;
}
