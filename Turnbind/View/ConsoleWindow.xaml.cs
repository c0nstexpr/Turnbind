using Wpf.Ui.Controls;

namespace Turnbind.View;

public partial class ConsoleWindow : FluentWindow
{
    readonly LogTextBlock m_logTextBlock = App.GetService<LogTextBlock>();

    public ConsoleWindow()
    {
        InitializeComponent();
        DockPanel.Children.Add(App.GetService<LogTextBlock>());
    }

    protected override void OnClosed(EventArgs e)
    {
        DockPanel.Children.Remove(m_logTextBlock);
        m_logTextBlock.LogTextBox.Document.Blocks.Clear();
        base.OnClosed(e);
    }
}
