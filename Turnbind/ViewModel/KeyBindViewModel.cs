using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class KeyBindViewModel : ObservableValidator, IEquatable<KeyBindViewModel>
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
    double m_pixelPerSec;

    [Range(double.Epsilon, double.MaxValue)]
    public string PixelPerSecString
    {
        get => PixelPerSec.ToString();

        set
        {
            ValidateProperty(value);
            if (GetErrors().Any()) return;
            PixelPerSec = double.Parse(value);
        }
    }

    public TurnSetting TurnSetting => new()
    {
        Dir = Dir,
        PixelPerSec = PixelPerSec
    };

    public bool Equals(KeyBindViewModel? other) => other is { } &&
        Keys.Equals(other.Keys) &&
        Dir == other.Dir &&
        PixelPerSec == other.PixelPerSec;

    public override bool Equals(object? obj) => Equals(obj as KeyBindViewModel);

    public override int GetHashCode() =>
        HashCode.Combine(Keys.GetHashCode(), Dir.GetHashCode(), PixelPerSec.GetHashCode());
}
