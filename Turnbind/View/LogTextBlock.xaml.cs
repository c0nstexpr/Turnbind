using System.Windows.Controls;

namespace Turnbind.View;

public partial class LogTextBlock : UserControl
{
    public LogTextBlock() => InitializeComponent();

    void OnTextChanged(object sender, TextChangedEventArgs e) => LogTextBox.ScrollToEnd();
}
