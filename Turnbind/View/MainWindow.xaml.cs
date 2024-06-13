using System.Windows;
using System.Windows.Interop;

using Turnbind.Action;
using Turnbind.ViewModel;

using Windows.Win32;

using Wpf.Ui.Controls;

namespace Turnbind.View;

sealed partial class MainWindow : FluentWindow
{
    readonly MainWindowViewModel m_viewModel;

    readonly TurnTickAction m_turnTickAction;

    ConsoleWindow? m_consoleWindow;

    public MainWindow()
    {
        InitializeComponent();

        m_viewModel = App.GetRequiredService<MainWindowViewModel>();
        m_turnTickAction = App.GetRequiredService<TurnTickAction>();

        DataContext = m_viewModel;

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

    void LaunchConsoleWindow(object sender, RoutedEventArgs e)
    {
        if (m_consoleWindow is { }) return;

        m_consoleWindow = new ConsoleWindow();
        m_consoleWindow.Closed += (_, _) => m_consoleWindow = null;
        m_consoleWindow.Show();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        m_turnTickAction.WinSrc = PresentationSource.FromVisual(this) as HwndSource;
    }

    protected override void OnClosed(EventArgs e)
    {
        Tray.Dispose();
        KeyBindsControl.Dispose();
        m_viewModel.Dispose();
        m_turnTickAction.WinSrc = null;
        base.OnClosed(e);
    }
}