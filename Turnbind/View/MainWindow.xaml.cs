using System.Windows;

using Turnbind.ViewModel;

using Wpf.Ui.Controls;

namespace Turnbind.View;

sealed partial class MainWindow : FluentWindow
{
    readonly MainWindowViewModel m_viewModel = App.GetService<MainWindowViewModel>();

    public MainWindow()
    {
        DataContext = m_viewModel;
        InitializeComponent();

        if (!m_viewModel.AdminSuggestEnable)
            MainDockPanel.Children.Remove(AdminSuggest);
    }

    protected override void OnClosed(EventArgs e)
    {
        KeyBindsControl.Dispose();
        m_viewModel.Dispose();
        base.OnClosed(e);
    }

    void LaunchConsoleWindow(object sender, RoutedEventArgs e)
    {
        ConsoleWindow consoleWindow = new();
        consoleWindow.Show();
    }
}