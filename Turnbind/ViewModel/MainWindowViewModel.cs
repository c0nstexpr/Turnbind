
using System.Reactive.Disposables;
using System.Reactive.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Action;
using Turnbind.Model;

namespace Turnbind.ViewModel;

internal class MainWindowViewModel : ObservableObject, IDisposable
{
    readonly ProcessWindowAction m_windowAction;

    readonly InputAction m_inputAction;

    readonly CompositeDisposable m_disposables;

    public string? ProcessName
    {
        get => m_windowAction.ProcessName;
        
        set
        {
            m_windowAction.ProcessName = value;
            OnPropertyChanged();
        }
    }

    public string IsWindowFocused => m_windowAction.IsFocused ? "Yes" : "No";

    readonly List<InputKey> m_inputKeys = new(Enum.GetValues<InputKey>().Length);

    public MainWindowViewModel(ProcessWindowAction windowAction, InputAction inputAction)
    {
        m_windowAction = windowAction;
        m_inputAction = inputAction;

        m_disposables = new()
        {
            m_inputAction.Input.Subscribe(OnInput),
            m_windowAction.Focused.Subscribe(_ => OnPropertyChanged(nameof(IsWindowFocused)))
        };
    }

    public string CurrentKeyStr => string.Join(" + ", m_inputKeys);

    void OnInput(InputAction.KeyState state)
    {
        if (state.Pressed) m_inputKeys.Add(state.Key);
        else m_inputKeys.Remove(state.Key);

        OnPropertyChanged(nameof(CurrentKeyStr));
    }

    public void Dispose() => m_disposables.Dispose();
}
