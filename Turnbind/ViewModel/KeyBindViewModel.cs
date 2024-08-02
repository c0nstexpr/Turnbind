using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindViewModel : ObservableValidator
{
    InputKeys m_keys = new();

    public InputKeys Keys
    {
        get => m_keys;

        set
        {
            SetProperty(ref m_keys, value);
            OnPropertyChanged(nameof(KeysString));
        }
    }

    public string KeysString => string.Join(" + ", ((IEnumerable<InputKey>)m_keys).Select(k => $"{k}"));

    [ObservableProperty]
    TurnDirection m_dir;

    [ObservableProperty]
    [Range(double.Epsilon, double.MaxValue)]
    double m_pixelPerMs;

    [ObservableProperty]
    [Range(double.Epsilon, double.MaxValue)]
    double m_wheelFactor;

    public TurnSetting TurnSetting
    {
        set
        {
            Dir = value.Dir;
            PixelPerMs = value.PixelPerMs;
            WheelFactor = value.WheelFactor;
        }

        get => new()
        {
            Dir = Dir,
            PixelPerMs = PixelPerMs,
            WheelFactor = WheelFactor
        };
    }
}
