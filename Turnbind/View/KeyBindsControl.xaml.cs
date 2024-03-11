using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

public partial class KeyBindsControl : UserControl
{
    internal readonly KeyBindsViewModel m_viewModel = new();

    public KeyBindsControl()
    {
        DataContext = m_viewModel;

        InitializeComponent();

    }
}
