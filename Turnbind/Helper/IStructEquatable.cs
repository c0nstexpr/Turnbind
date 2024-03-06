namespace Turnbind.Helper;

public interface IStructEquatable<T> : IEquatable<T> where T : struct
{
    public bool Equals(object? obj) => Equals(obj as T?);
}
