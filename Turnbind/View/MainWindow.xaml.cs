using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.ViewModel;

namespace Turnbind.View;

[INotifyPropertyChanged]
sealed partial class MainWindow : Window
{
    readonly MainWindowViewModel m_viewModel = App.GetService<MainWindowViewModel>();

    public MainWindow()
    {
        DataContext = m_viewModel;
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        KeyBindsControl.Dispose();
        m_viewModel.Dispose();
        base.OnClosed(e);
    }
}