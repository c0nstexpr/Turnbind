using System.Collections.Specialized;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.ViewModel;

namespace Turnbind.View;

[INotifyPropertyChanged]
public partial class MainWindow : Window
{
    TurnBindViewModel m_turnBindVM => TurnBindControl.m_viewModel;

    readonly MainWindowViewModel m_viewModel = new();

    public MainWindow()
    {
        DataContext = m_viewModel;
        InitializeComponent();
        //WithTurnBindVm();

        //Closed += (_, _) =>
        //{
        //    m_viewModel.Dispose();
        //    m_turnBindVM.Binds.CollectionChanged -= OnBindingsChanged;
        //    TurnBindControl.Dispose();
        //};
    }

    void WithTurnBindVm()
    {
        //if (m_turnBindVM is null || m_viewModel is null) return;

        //m_viewModel.InputAction = m_turnBindVM.InputAction;

        //foreach (var bind in m_turnBindVM.Binds)
        //    m_viewModel.AddBind(bind);

        //m_turnBindVM.Binds.CollectionChanged += OnBindingsChanged;
    }

    void OnBindingsChanged(object? _, NotifyCollectionChangedEventArgs args)
    {
        //switch (args.Action)
        //{
        //    case NotifyCollectionChangedAction.Add:
        //        m_viewModel?.AddBind(args.NewItems!.Cast<BindingViewModel>().First());
        //        break;
        //    case NotifyCollectionChangedAction.Remove:
        //        m_viewModel?.RemoveBind(args.OldItems!.Cast<BindingViewModel>().First());
        //        break;
        //}
    }
}