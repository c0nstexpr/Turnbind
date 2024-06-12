using System.Windows;
using System.Windows.Controls;

namespace Turnbind.View;

public partial class LogTextBlock : UserControl
{
    public LogTextBlock() => InitializeComponent();

    bool m_autoScroll = true;

    void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (m_autoScroll) LogTextBox.ScrollToEnd();
    }

    void OnScroll(object sender, ScrollChangedEventArgs e)
    {
        var verticalChange = e.VerticalChange;

        if (verticalChange > 0 && e.ExtentHeight - e.VerticalOffset - e.ViewportHeight < 1)
            m_autoScroll = true;
        else if (verticalChange < 0)
            m_autoScroll = false;
    }

    void ClearButtonClick(object sender, RoutedEventArgs e) => LogTextBox.Document.Blocks.Clear();
}
