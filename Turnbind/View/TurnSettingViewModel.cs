using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.View;

partial class TurnSettingViewModel : ObservableObject
{
    public TurnSetting Setting => new()
    {
        Dir = Dir,
        PixelPerSec = PixelPerSec
    };

    [ObservableProperty]
    TurnDirection m_dir;

    [ObservableProperty]
    double m_pixelPerSec;
}
