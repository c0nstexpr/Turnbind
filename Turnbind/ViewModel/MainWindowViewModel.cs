using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Action;
using Turnbind.Model;

namespace Turnbind.ViewModel;

internal partial class MainWindowViewModel : ObservableValidator, IDisposable
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

    public string IsWindowFocused => m_windowAction.Focused.Value ? "Focused" : "Lost focus";

    readonly List<InputKey> m_inputKeys = new(Enum.GetValues<InputKey>().Length);

    public string CurrentKeyStr => string.Join(" + ", m_inputKeys);

    public int CurrentMousePosX => m_inputAction.Point.X;

    public int CurrentMousePosY => m_inputAction.Point.Y;


    [Range(1.0, uint.MaxValue)]
    public string TurnInterval
    {
        get => m_turnAction.Interval.TotalMilliseconds.ToString();

        set
        {
            ValidateProperty(value);
            if (GetErrors().Any()) return;

            var interval = double.Parse(value);
            m_turnAction.Interval = TimeSpan.FromMilliseconds(interval);
            m_settings.TurnInterval = interval;
        }
    }

    public string Instruction => m_turnAction.Direction switch
    {
        TurnInstruction.Stop => "Stopped",
        TurnInstruction.Left => "Turning left",
        TurnInstruction.Right => "Turning right",
        _ => throw new ArgumentOutOfRangeException(nameof(Instruction))
    };

    public MainWindowViewModel(Settings settings, ProcessWindowAction windowAction, InputAction inputAction, TurnAction turnAction)
    {
        m_settings = settings;
        m_windowAction = windowAction;
        m_inputAction = inputAction;
        m_turnAction = turnAction;

        m_disposables = [
            m_inputAction.KeyboardInput.Subscribe(OnKeyboard),
            m_inputAction.MouseMove.Subscribe(
                _ =>
                {
                    OnPropertyChanged(nameof(CurrentMousePosX));
                    OnPropertyChanged(nameof(CurrentMousePosY));
                }
            ),
            m_turnAction.WhenChanged(action => action.Direction).Subscribe(_ => OnPropertyChanged(nameof(Instruction))),
            m_windowAction.Focused.Subscribe(_ => OnPropertyChanged(nameof(IsWindowFocused)))
        ];

        m_windowAction.ProcessName = m_settings.ProcessName;
        m_turnAction.Interval = TimeSpan.FromMilliseconds(m_settings.TurnInterval);
    }

    void OnKeyboard(InputAction.KeyState state)
    {
        if (state.Pressed) m_inputKeys.Add(state.Key);
        else m_inputKeys.Remove(state.Key);

        OnPropertyChanged(nameof(CurrentKeyStr));
    }

    [RelayCommand]
    static void OnExit() => App.Current.Shutdown();

    public void Dispose()
    {
        m_disposables.Dispose();
        m_settings.Save();
    }
}
