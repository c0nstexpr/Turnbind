using System.Collections.Specialized;
using System.Data.Common;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;
using Turnbind.ViewModel;

namespace Turnbind.View;

[INotifyPropertyChanged]
public partial class TurnBindControl : UserControl, IDisposable
{
    internal readonly TurnBindViewModel ViewModel = new();

    public TurnBindControl()
    {
        DataContext = ViewModel;

        InitializeComponent();

    }

    readonly SerialDisposable m_bindingKeysDisposable = new();

    void BindingKeysTextBoxGotFocus(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null) return;

        //ViewModel.ClearKeys();

        //m_bindingKeysDisposable.Disposable = Synchronization.ObserveOn(
        //    ViewModel.InputAction.Input.Skip(1),
        //    new DispatcherSynchronizationContext(Dispatcher)
        //)
        //    .Where(_ => BindingKeysTextBox.IsFocused)
        //    .Subscribe(
        //        s =>
        //        {
        //            var (k, p) = s;

        //            switch (k)
        //            {
        //                case InputKey.Enter or InputKey.NumEnter:
        //                    BindingKeysTextBox.MoveFocus(new(FocusNavigationDirection.Next));
        //                    return;

        //                case InputKey.Escape:
        //                    BindingKeysTextBox.MoveFocus(new(FocusNavigationDirection.Previous));
        //                    return;

        //                case InputKey.Backspace:
        //                    ViewModel.ClearKeys();
        //                    return;

        //                default:
        //                    ViewModel.OnKey(k, p);
        //                    return;
        //            }
        //        }
        //    );
    }

    //void BindingKeysTextBoxLostFocus(object sender, RoutedEventArgs e) => m_bindingKeysDisposable.Disposable = null;

    //void ModifyButtonClick(object sender, RoutedEventArgs e)
    //{
    //    if (ViewModel?.Modify() == true) return;

    //    MessageBox.Show("Bind with same keys already exists.\n Select it if you need to modify.");
    //}

    //void RemoveButtonClick(object sender, RoutedEventArgs e) => ViewModel?.Remove();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    bool m_disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || m_disposed) return;

        //ViewModel.Dispose();
        m_bindingKeysDisposable.Dispose();
        m_disposed = true;
    }

    public void ProfileAddButtonClick(object sender, RoutedEventArgs e)
    {

    }

    public void ProfileRemoveButtonClick(object sender, RoutedEventArgs e)
    {

    }

    public void ModifyButtonClick(object sender, RoutedEventArgs e)
    {

    }

    public void RemoveButtonClick(object sender, RoutedEventArgs e)
    {

    }

    public void BindingKeysTextBoxLostFocus(object sender, RoutedEventArgs e)
    {

    }
}
