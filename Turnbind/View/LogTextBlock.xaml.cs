using System.Windows;
using System.Windows.Controls;

namespace Turnbind.View;

public partial class LogTextBlock : UserControl
{
    bool m_autoScroll = true;

    public LogTextBlock()
    {
        DataContext = this;
        InitializeComponent();
    }

    void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (m_autoScroll) LogTextBox.ScrollToEnd();
    }

    void OnScroll(object sender, ScrollChangedEventArgs e)
    {
        if ((e.OriginalSource as ScrollViewer)?.CanContentScroll != true)
        {
            m_autoScroll = true;
            return;
        }

        var verticalChange = e.VerticalChange;

        if (verticalChange > 0 && e.ExtentHeight - e.VerticalOffset - e.ViewportHeight < 1)
            m_autoScroll = true;
        else if (verticalChange < 0)
            m_autoScroll = false;
    }

    void ClearButtonClick(object sender, RoutedEventArgs e) => LogTextBox.Document.Blocks.Clear();
}
