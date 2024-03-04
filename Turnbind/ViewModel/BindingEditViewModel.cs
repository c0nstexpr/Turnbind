using CommunityToolkit.Mvvm.ComponentModel;
using Turnbind.Model;

namespace Turnbind.ViewModel;
internal partial class BindingEditViewModel : ObservableObject
{
    public TurnDirection TurnDirection
    {
        get => Binding.Dir;

        set
        {
            Binding.Dir = value;
            OnPropertyChanged();
        }
    }

    public double PixelPerSec
    {
        get => Binding.PixelPerSec;

        set
        {
            Binding.PixelPerSec = value;
            OnPropertyChanged();
        }
    }

    public string? BindingKeys => Binding.KeysString;


    BindingViewModel m_binding = new();

    public BindingViewModel Binding
    {
        get => m_binding;

        set
        {
            SetProperty(ref m_binding, value);
            OnPropertyChanged(nameof(TurnDirection));
            OnPropertyChanged(nameof(PixelPerSec));
            OnPropertyChanged(nameof(BindingKeys));
        }
    }

    public void OnKey(InputKey k, bool p)
    {
        if (!p) return;

        Binding.Keys.Add(k);
        OnPropertyChanged(nameof(BindingKeys));
    }

    public void ClearKeys()
    {
        Binding.Keys.Clear();
        OnPropertyChanged(nameof(BindingKeys));
    }
}
