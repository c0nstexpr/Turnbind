namespace Turnbind.Model;

public class TurnSetting : IEquatable<TurnSetting>
{
    public TurnDirection Dir { get; set; }

    public double PixelPerSec { get; set; }

    public bool Equals(TurnSetting? other) =>
        other is { } && Dir == other.Dir && PixelPerSec == other.PixelPerSec;

    public override bool Equals(object? obj) => Equals(obj as TurnSetting);

    public override int GetHashCode() => HashCode.Combine(Dir, PixelPerSec);
}
