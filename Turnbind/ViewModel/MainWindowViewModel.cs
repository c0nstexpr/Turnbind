
using System.Reactive.Disposables;
using System.Reactive.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Action;
using Turnbind.Model;

namespace Turnbind.ViewModel;

internal class MainWindowViewModel : ObservableObject, IDisposable
{
    readonly Settings m_settings;

    readonly ProcessWindowAction m_windowAction;

    readonly InputAction m_inputAction;

    readonly TurnAction m_turnAction;

    readonly CompositeDisposable m_disposables;

    public string? ProcessName
    {
        get => m_windowAction.ProcessName;

        set
        {
            m_windowAction.ProcessName = value;
            m_settings.ProcessName = value ?? "";
            OnPropertyChanged();
        }
    }

    public string IsWindowFocused => m_windowAction.Focused.Value ? "Yes" : "No";

    readonly List<InputKey> m_inputKeys = new(Enum.GetValues<InputKey>().Length);

    public MainWindowViewModel(Settings settings, ProcessWindowAction windowAction, InputAction inputAction, TurnAction turnAction)
    {
        m_settings = settings;
        m_windowAction = windowAction;
        m_inputAction = inputAction;
        m_turnAction = turnAction;

        m_disposables = [
            m_inputAction.Input.Subscribe(OnInput),
            m_windowAction.Focused.Subscribe(_ => OnPropertyChanged(nameof(IsWindowFocused)))
        ];

        m_windowAction.ProcessName = m_settings.ProcessName;
        m_turnAction.Interval = TimeSpan.FromMilliseconds(m_settings.TurnInterval);
    }

    public string CurrentKeyStr => string.Join(" + ", m_inputKeys);

    void OnInput(InputAction.KeyState state)
    {
        if (state.Pressed) m_inputKeys.Add(state.Key);
        else m_inputKeys.Remove(state.Key);

        OnPropertyChanged(nameof(CurrentKeyStr));
    }

    public double TurnInterval
    {
        get => m_turnAction.Interval.TotalMilliseconds;

        set
        {
            m_turnAction.Interval = TimeSpan.FromMilliseconds(value);
            m_settings.TurnInterval = value;
            OnPropertyChanged();
        }
    }

    public void Dispose()
    {
        m_disposables.Dispose();
        m_settings.Save();
    }
}
