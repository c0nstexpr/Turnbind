namespace Turnbind.ViewModel
{
    using CommunityToolkit.Mvvm.ComponentModel;

    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using Turnbind.Model;

    internal partial class TurnBindViewModel : ObservableObject
    {
        #region Bind settings

        [ObservableProperty]
        TurnDirection _turnDirection;

        [ObservableProperty]
        double _pixelPerSec;

        readonly List<InputKey> _newBindingKeys = [];

        string? _bindingKeys;

        public string? BindingKeys
        {
            get => _bindingKeys;
            private set => SetProperty(ref _bindingKeys, value);
        }

        #endregion

        #region Bind list

        public ObservableCollection<TurnBindKeys> TurnBindKeysList { get; } = [];

        int _focusedBindingIndex;

        public int FocusedBindingIndex
        {
            get => _focusedBindingIndex;

            set
            {
                SetProperty(ref _focusedBindingIndex, value);
                OnPropertyChanged(nameof(FocusedBinding));
                OnPropertyChanged(nameof(ModifyButtonContent));
            }
        }

        TurnBindKeys? FocusedBinding => FocusedBindingIndex >= 0 ? TurnBindKeysList[FocusedBindingIndex] : null;

        #endregion

        public TurnSettings Settings { get; set; } = new();

        public string ModifyButtonContent => FocusedBinding is null ? "Add" : "Modify";

        public TurnBindViewModel()
        {
            TurnBindKeysList = new(Settings.Binds);

            TurnBindKeysList.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Settings.Binds.Insert(args.NewStartingIndex, args.NewItems!.Cast<TurnBindKeys>().First());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        Settings.Binds.RemoveAt(args.OldStartingIndex);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            };
        }

        public void OnKey(InputKey k, bool p)
        {
            if (!p) return;

            _newBindingKeys.Add(k);
            BindingKeys = string.Join(" + ", _newBindingKeys.Select(k => $"{k}"));
        }

        public void ClearKeys()
        {
            _newBindingKeys.Clear();
            BindingKeys = null;
        }

        public void Modify()
        {
            if (FocusedBinding == null)
            {
                TurnBindKeysList.Add(
                    new()
                    {
                        Dir = TurnDirection,
                        PixelPerSec = PixelPerSec,
                        Keys = [.. _newBindingKeys]
                    }
                );
            }
            else
            {
                FocusedBinding.Keys.Clear();
                FocusedBinding.Keys.AddRange(_newBindingKeys);
            }

            Settings.Save();
        }
    }
}
