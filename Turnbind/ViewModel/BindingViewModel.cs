using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class BindingViewModel : ObservableObject
{
    [ObservableProperty]
    TurnDirection m_dir;

    public IEnumerable<InputKey> Keys
    {
        get => m_keys;
        set
        {
            SetProperty(ref m_keys, value);
            OnPropertyChanged(nameof(KeysString));
        }
    }

    public string KeysString => m_keys.ToKeyString();

    [ObservableProperty]
    double m_pixelPerSec;

    IEnumerable<InputKey> m_keys = [];

    public Binding Binding => new()
    {
        Dir = Dir,
        Keys = m_keys?.ToList() ?? [],
        PixelPerSec = PixelPerSec
    };
}
