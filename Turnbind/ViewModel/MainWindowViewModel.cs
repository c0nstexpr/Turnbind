
using System.Reactive.Disposables;
using System.Reactive.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using MoreLinq;

using Turnbind.Action;
using Turnbind.Model;

namespace Turnbind.ViewModel;

internal class MainWindowViewModel : ObservableObject, IDisposable
{
    readonly Dictionary<Binding, SerialDisposable> m_bindDisposables = [];

    IDisposable m_keyWatchDisposable = Disposable.Empty;

    private InputAction? m_inputAction;

    readonly TurnAction m_turnAction = new();

    readonly ProcessWindowAction m_windowAction = new();

    readonly IDisposable m_windowFocusedDisposable;

    public MainWindowViewModel() => m_windowFocusedDisposable = m_windowAction.Focused
        .Subscribe(focused => OnPropertyChanged(nameof(IsWindowFocused)));

    public string? ProcessName { get => m_windowAction.ProcessName; set => m_windowAction.ProcessName = value; }

    public string CurrentKeyStr => string.Join(" + ", m_inputKeys);

    public string IsWindowFocused => m_windowAction.IsFocused ? "Yes" : "No";

    readonly List<InputKey> m_inputKeys = new(Enum.GetValues<InputKey>().Length);

    public InputAction? InputAction
    {
        get => m_inputAction;
        
        set
        {
            m_inputAction = value;

            if (m_inputAction == null) return;

            m_keyWatchDisposable.Dispose();
            m_keyWatchDisposable = m_inputAction.Input.Subscribe(OnInput);
        }
    }

    void OnInput(InputAction.KeyState state)
    {
        if(state.Pressed)
            m_inputKeys.Add(state.Key);
        else
            m_inputKeys.Remove(state.Key);

        OnPropertyChanged(nameof(CurrentKeyStr));
    }

    public void AddBind(BindingViewModel bind)
    {
        var b = bind.Binding;
        if (!m_bindDisposables.TryGetValue(b, out var disposable))
            disposable = new();

        disposable.Disposable = SubscribeBind(b);
    }

    public void RemoveBind(BindingViewModel bind)
    {
        var b = bind.Binding;
        if (!m_bindDisposables.TryGetValue(b, out var disposable))
            return;

        disposable.Dispose();
        m_bindDisposables.Remove(b);
    }

    IDisposable SubscribeBind(Binding bind)
    {
        var turnObservable = m_turnAction.Turn(bind.Dir, bind.PixelPerSec);
        var disposable = Disposable.Empty;

        return new CompositeDisposable()
        {
            InputAction?.SubscribeKeys(bind.Keys).Subscribe(
                active =>
                {
                    disposable.Dispose();

                    if (active && m_windowAction.IsFocused) disposable = turnObservable.Subscribe();
                }
            ) ?? Disposable.Empty,
            disposable
        };
    }

    public void Dispose()
    {
        m_windowAction.Dispose();
        m_bindDisposables.Values.ForEach(d => d.Dispose());
        m_keyWatchDisposable.Dispose();
        m_windowFocusedDisposable.Dispose();
    }
}
