namespace Turnbind.Model;

public class Binding : IEquatable<Binding>
{
    public TurnDirection Dir { get; set; }

    public List<InputKey> Keys { get; set; } = [];

    public double PixelPerSec { get; set; }

    public bool Equals(Binding? other)
    {
        var otherKeys = other?.Keys;

        if (Keys.Count != otherKeys?.Count)
            return false;

        for (var i = 0; i < Keys.Count; i++)

            if (Keys[i] != otherKeys[i])
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        HashCode keysCode = new();

        foreach (var key in Keys)
            keysCode.Add(key);

        return keysCode.ToHashCode();
    }

}