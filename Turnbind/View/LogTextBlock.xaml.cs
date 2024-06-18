using System.Windows;
using System.Windows.Controls;

namespace Turnbind.View;

public partial class LogTextBlock : UserControl
{
    bool m_autoScroll = true;

    public bool AutoScroll
    {
        get => m_autoScroll;

        set
        {
            if (value) LogTextBox.ScrollToEnd();
            m_autoScroll = value;
        }
    }

    public LogTextBlock()
    {
        DataContext = this;
        InitializeComponent();

        IsEnabled = false;
    }

    void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.Changes.All(c => c.AddedLength == 0)) return;

        if (Parent is null)
        {
            LogTextBox.Document.Blocks.Clear();
            return;
        }

        if (AutoScroll) LogTextBox.ScrollToEnd();
    }

    void OnScroll(object sender, ScrollChangedEventArgs e)
    {
        if ((e.OriginalSource as ScrollViewer)?.CanContentScroll != true)
        {
            AutoScroll = true;
            return;
        }

        var verticalChange = e.VerticalChange;

        if (verticalChange > 0 && e.ExtentHeight - e.VerticalOffset - e.ViewportHeight < 1)
            AutoScroll = true;
        else if (verticalChange < 0)
            AutoScroll = false;
    }

    void ClearButtonClick(object sender, RoutedEventArgs e) => LogTextBox.Document.Blocks.Clear();
}
