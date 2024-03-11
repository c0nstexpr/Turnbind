using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

public partial class KeyBindListControl : UserControl
{
    internal readonly KeyBindListViewModel m_viewModel = new();

    public KeyBindListControl()
    {
        DataContext = m_viewModel;
        InitializeComponent();
        m_viewModel.KeyBindEdit = KeyBindEdit.m_viewModel;
    }
}
