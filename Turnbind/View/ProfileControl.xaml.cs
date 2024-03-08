using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

public partial class ProfileControl : UserControl
{
    internal readonly ProfileControlViewModel m_viewModel = new();

    public ProfileControl()
    {
        DataContext = m_viewModel;
        InitializeComponent();
    }
}
