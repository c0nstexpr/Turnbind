using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Linq;

using Turnbind.Action;
using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class TurnBindViewModel : ObservableObject, IDisposable
{
    public readonly InputAction InputAction = new();

    #region Bind settings

    [ObservableProperty]
    TurnDirection m_turnDirection;

    [ObservableProperty]
    double m_pixelPerSec;

    readonly List<InputKey> m_newBindingKeys = [];

    string? m_bindingKeys;

    public string? BindingKeys
    {
        get => m_bindingKeys;
        private set => SetProperty(ref m_bindingKeys, value);
    }

    #endregion

    #region Bind list

    public ObservableCollection<BindingViewModel> Binds { get; } = [];

    int m_focusedBindingIndex = -1;

    public int FocusedBindingIndex
    {
        get => m_focusedBindingIndex;

        set
        {
            SetProperty(ref m_focusedBindingIndex, value);
            OnPropertyChanged(nameof(m_focusedBinding));
            OnPropertyChanged(nameof(ModifyButtonContent));
            OnPropertyChanged(nameof(RemoveButtonEnabled));

            if (m_focusedBinding is null) return;

            TurnDirection = m_focusedBinding.Dir;
            PixelPerSec = m_focusedBinding.PixelPerSec;
            BindingKeys = m_focusedBinding.Keys.ToKeyString();
        }
    }

    BindingViewModel? m_focusedBinding => FocusedBindingIndex >= 0 && FocusedBindingIndex < Binds.Count ?
        Binds[FocusedBindingIndex] : null;

    #endregion

    TurnSettings m_settings = new();

    public TurnSettings Settings
    {
        get => m_settings;

        set
        {
            SetProperty(ref m_settings, value);

            m_onTurnBindKeysListChangedCalled = true;

            Binds.Clear();

            foreach (var bind in m_binds)
                Binds.Add(
                    new()
                    {
                        Dir = bind.Dir,
                        PixelPerSec = bind.PixelPerSec,
                        Keys = bind.Keys
                    }
                );

            m_onTurnBindKeysListChangedCalled = false;
        }
    }

    HashSet<Binding> m_binds => Settings.Binds;

    public string ModifyButtonContent => m_focusedBinding is null ? "Add" : "Modify";

    public bool RemoveButtonEnabled => m_focusedBinding is not null;

    public TurnBindViewModel()
    {
        Binds = new(
            m_binds.Select(
                b => new BindingViewModel()
                {
                    Dir = b.Dir,
                    PixelPerSec = b.PixelPerSec,
                    Keys = b.Keys
                }
            )
        );
        Binds.CollectionChanged += OnTurnBindKeysListChanged;
    }

    bool m_onTurnBindKeysListChangedCalled = false;

    void OnTurnBindKeysListChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (m_onTurnBindKeysListChangedCalled) return;

        m_onTurnBindKeysListChangedCalled = true;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                m_binds.Add(GetElement(args.NewItems!));
                break;
            case NotifyCollectionChangedAction.Remove:
                m_binds.Remove(GetElement(args.OldItems!));
                break;
            default:
                break;
        }

        m_onTurnBindKeysListChangedCalled = false;
    }

    static Binding GetElement(IList items) => items.Cast<BindingViewModel>().First().Binding;

    public void OnKey(InputKey k, bool p)
    {
        if (!p) return;

        m_newBindingKeys.Add(k);
        BindingKeys = m_newBindingKeys.ToKeyString();
    }

    public void ClearKeys()
    {
        m_newBindingKeys.Clear();
        BindingKeys = null;
    }

    public bool Modify()
    {
        Binding dummyBind = new() { Keys = m_newBindingKeys };
        if (m_binds.Contains(dummyBind))
            return false;

        if (m_focusedBinding == null)
        {
            BindingViewModel newBind = new()
            {
                Dir = TurnDirection,
                PixelPerSec = PixelPerSec,
                Keys = [.. m_newBindingKeys]
            };

            Binds.Add(newBind);
        }
        else
        {
            m_binds.Remove(dummyBind);

            m_focusedBinding.Keys = [.. m_newBindingKeys];
            m_focusedBinding.Dir = TurnDirection;
            m_focusedBinding.PixelPerSec = PixelPerSec;

            m_binds.Add(m_focusedBinding.Binding);
        }

        Settings.Save();

        return true;
    }

    public void Remove()
    {
        Debug.Assert(RemoveButtonEnabled);
        Binds.RemoveAt(FocusedBindingIndex);
        Settings.Save();
    }

    public void Dispose() => InputAction.Dispose();
}
