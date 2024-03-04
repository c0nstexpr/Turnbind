namespace Turnbind.Model;
public class KeyList : List<InputKey>, IEquatable<KeyList>
{
    public bool Equals(KeyList? other) => Count != other?.Count ? false : this.SequenceEqual(other);

    public override int GetHashCode()
    {
        HashCode keysCode = new();

        foreach (var key in this)
            keysCode.Add(key);

        return keysCode.ToHashCode();
    }
}
