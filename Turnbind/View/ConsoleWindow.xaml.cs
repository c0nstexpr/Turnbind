using Wpf.Ui.Controls;

namespace Turnbind.View;

public partial class ConsoleWindow : FluentWindow
{
    readonly LogTextBlock m_logTextBlock = App.GetRequiredService<LogTextBlock>();

    public ConsoleWindow()
    {
        InitializeComponent();
        DockPanel.Children.Add(m_logTextBlock);

        m_logTextBlock.AutoScroll = true;
        m_logTextBlock.IsEnabled = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        DockPanel.Children.Remove(m_logTextBlock);
        m_logTextBlock.LogTextBox.Document = new();
        m_logTextBlock.IsEnabled = false;
        base.OnClosed(e);
    }
}
