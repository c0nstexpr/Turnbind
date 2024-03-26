using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Turnbind.Action;
using Turnbind.Model;

using Wpf.Ui.Controls;

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

    //public bool AdminSuggestEnable { get; }

    //[ObservableProperty]
    //bool m_isAdminSuggestFlyoutOpen;

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

        //using var identity = WindowsIdentity.GetCurrent();
        //var principal = new WindowsPrincipal(identity);
        //AdminSuggestEnable = !principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public string CurrentKeyStr => string.Join(" + ", m_inputKeys);

    void OnInput(InputAction.KeyState state)
    {
        if (state.Pressed) m_inputKeys.Add(state.Key);
        else m_inputKeys.Remove(state.Key);

        OnPropertyChanged(nameof(CurrentKeyStr));
    }

    public static uint MaxTurnInterval { get; } = uint.MaxValue - 1;

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

    //[RelayCommand]
    //void OnAdminSuggestButtonClick() => IsAdminSuggestFlyoutOpen = !IsAdminSuggestFlyoutOpen;

    //[RelayCommand]
    //static void RestartAsAdmin()
    //{
    //    Process.Start(
    //        new ProcessStartInfo()
    //        {
    //            UseShellExecute = true,
    //            FileName = Environment.ProcessPath,
    //            Verb = "runas",
    //            CreateNoWindow = false,
    //            ErrorDialog = true
    //        }
    //    );
    //    Application.Current.Shutdown();
    //}

    [RelayCommand]
    static void OnExit() => Application.Current.Shutdown();

    public void Dispose()
    {
        m_disposables.Dispose();
        m_settings.Save();
    }
}
