
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

    readonly TurnAction m_turnAction = new();

    readonly ProcessWindowAction m_windowAction = new();

    public string? ProcessName { get => m_windowAction.ProcessName; set => m_windowAction.ProcessName = value; }

    public InputAction? InputAction { get; set; }

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

                    if (active) disposable = turnObservable.Subscribe();
                }
            ) ?? Disposable.Empty,
            disposable
        };
    }

    public void Dispose()
    {
        m_windowAction.Dispose();
        m_bindDisposables.Values.ForEach(d => d.Dispose());
    }
}
