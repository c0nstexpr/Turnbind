namespace Turnbind.Model;

public struct TurnSetting
{
    public TurnDirection Dir { get; set; }

    public double PixelPerMs { get; set; }

    public double WheelFactor { get; set; }
}
