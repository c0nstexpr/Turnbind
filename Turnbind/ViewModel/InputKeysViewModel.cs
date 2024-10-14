using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class InputKeysViewModel : ObservableObject
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

        Keys = new(Keys.Concat([k]));
    }
}
