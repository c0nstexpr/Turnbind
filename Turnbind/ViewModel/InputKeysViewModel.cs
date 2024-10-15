using System.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class InputKeysViewModel : ObservableObject, IList<InputKey>, IList
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

    public void OnInputKey(InputKey k)
    {
        if (Keys.Contains(k)) return;
        Keys = new(Keys.Concat(new[] { k }));
    }

    public InputKey this[int i]
    {
        get => Keys[i];

        set
        {
            if (Keys.ContainsKey(value)) throw new ArgumentException(nameof(value));

            var arr = new InputKey[Keys.Count];

            for (var j = 0; j < i; ++j)
                arr[j] = Keys[j];

            arr[i] = value;

            for (var j = i + 1; j < Keys.Count; ++j)
                arr[j] = Keys[j];

            Keys = new(arr);
        }
    }

    public int Count => Keys.Count;

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public bool IsSynchronized => false;

    public object SyncRoot => Keys;
    public IEnumerator<InputKey> GetEnumerator() => Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(InputKey item) => Keys.TryGetValue(item, out var i) ? i : -1;

    public void Insert(int index, InputKey item)
    {
        if (Keys.Contains(item)) throw new ArgumentException(nameof(item));

        var arr = new InputKey[Keys.Count + 1];

        for (var i = 0; i < index; ++i)
            arr[i] = Keys[i];

        arr[index] = item;

        for (var i = index; i < Keys.Count; ++i)
            arr[i + 1] = Keys[i];

        Keys = new(arr);
    }

    public void RemoveAt(int i)
    {
        var arr = new InputKey[Keys.Count - 1];

        for (var j = 0; j < i; ++j)
            arr[j] = Keys[j];

        for (var j = i + 1; j < Keys.Count; ++j)
            arr[j - 1] = Keys[j];

        Keys = new(arr);
    }

    public void Add(InputKey item) => OnInputKey(item);

    public void Clear() => Keys = new();

    public bool Contains(InputKey item) => Keys.ContainsKey(item);

    public void CopyTo(InputKey[] array, int i)
    {
        foreach (var key in Keys) array[i++] = key;
    }

    public bool Remove(InputKey item)
    {
        if (!Keys.TryGetValue(item, out var i)) return false;

        RemoveAt(i);

        return true;
    }
    object? IList.this[int i]
    {
        get => this[i];

        set => this[i] = (InputKey)value!;
    }

    public int Add(object? v)
    {
        Add((InputKey)v!);
        return Count - 1;
    }

    public bool Contains(object? value) => Contains((InputKey)value!);

    public int IndexOf(object? value) => IndexOf((InputKey)value!);

    public void Insert(int index, object? value) => Insert(index, (InputKey)value!);

    public void Remove(object? value) => Remove((InputKey)value!);

    public void CopyTo(Array array, int i)
    {
        foreach (var key in Keys) array.SetValue(key, i++);
    }
}
