using System.Windows;

using Turnbind.ViewModel;

using Wpf.Ui.Controls;

namespace Turnbind.View;

sealed partial class MainWindow : FluentWindow
{
    readonly MainWindowViewModel m_viewModel = App.GetRequiredService<MainWindowViewModel>();

    ConsoleWindow? m_consoleWindow;

    public MainWindow()
    {
        DataContext = m_viewModel;
        InitializeComponent();

        if (App.GetService<LogTextBlock>() is null)
            MainDockPanel.Children.Remove(LaunchConsoleButton);
    }

    protected override void OnClosed(EventArgs e)
    {
        Tray.Dispose();
        KeyBindsControl.Dispose();
        m_viewModel.Dispose();
        base.OnClosed(e);
    }

    void LaunchConsoleWindow(object sender, RoutedEventArgs e)
    {
        if (m_consoleWindow is { }) return;

        m_consoleWindow = new ConsoleWindow();
        m_consoleWindow.Closed += (_, _) => m_consoleWindow = null;
        m_consoleWindow.Show();
    }
}