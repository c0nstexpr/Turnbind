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

        // We don't use data binding for ContextMenu, because
        // https://stackoverflow.com/questions/1013558/elementname-binding-from-menuitem-in-contextmenu
        if (Tray.Menu is not { } menu)
        {
            menu = new();
            Tray.Menu = menu;
        }

        menu.Items.Add(new System.Windows.Controls.MenuItem() { Header = "Exit", Command = m_viewModel.ExitCommand });
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