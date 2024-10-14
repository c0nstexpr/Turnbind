using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class TurnSettingViewModel : ObservableValidator
{
    TurnSetting m_turnSetting = new();

    public TurnSetting TurnSetting
    {
        get => m_turnSetting;

        set
        {
            m_turnSetting = value;
            OnPropertyChanged(nameof(Dir));
            OnPropertyChanged(nameof(PixelPerMs));
            OnPropertyChanged(nameof(WheelFactor));
        }
    }

    public TurnDirection Dir
    {
        set => SetProperty(TurnSetting.Dir, value, v => TurnSetting.Dir = v);

        get => TurnSetting.Dir;
    }

    [Range(double.Epsilon, double.MaxValue)]
    public double PixelPerMs
    {
        set => SetProperty(TurnSetting.PixelPerMs, value, v => TurnSetting.PixelPerMs = v);

        get => TurnSetting.PixelPerMs;
    }

    [Range(double.Epsilon, double.MaxValue)]
    public double WheelFactor
    {
        set => SetProperty(TurnSetting.WheelFactor, value, v => TurnSetting.WheelFactor = v);

        get => TurnSetting.WheelFactor;
    }
}
