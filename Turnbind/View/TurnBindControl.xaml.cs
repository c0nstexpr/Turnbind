namespace Turnbind
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Turnbind.Action;
    using Turnbind.Model;
    using Turnbind.ViewModel;

    public partial class TurnBindControl : UserControl
    {
        internal InputAction InputAction = new();

        TurnBindViewModel _viewModel => (TurnBindViewModel)DataContext;

        public TurnBindControl()
        {
            InitializeComponent();

            {
                var items = TurnDicectionComboBox.Items;

                foreach(var item in Enum.GetValues<TurnDirection>())
                    items.Add(item);              
            }

            DataContext = new TurnBindViewModel();

            CompositeDisposable disposables = [];

            disposables.Add(
                InputAction.Input.Where(_ => BindingKeysLabel.IsFocused).Subscribe(
                    s =>
                    {
                        var (k, p) = s;

                        switch (k)
                        {
                            case InputKey.Enter:
                                BindingKeysLabel.MoveFocus(new(FocusNavigationDirection.Next));
                                return;

                            case InputKey.Escape:
                                BindingKeysLabel.MoveFocus(new(FocusNavigationDirection.Previous));
                                return;

                            case InputKey.Backspace:
                                _viewModel.ClearKeys();
                                return;

                            default:
                                _viewModel.OnKey(k, p);
                                return;
                        }
                    }
                )
            );

            disposables.Add(InputAction);

            Unloaded += (_, _) => disposables.Dispose();
        }

        void BindingKeysLabelGotFocus(object sender, RoutedEventArgs e) => _viewModel.ClearKeys();

        void ModifyButtonClick(object sender, RoutedEventArgs e) => _viewModel.Modify();
    }
}
