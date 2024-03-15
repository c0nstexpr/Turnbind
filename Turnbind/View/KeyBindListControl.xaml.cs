using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

partial class KeyBindListControl : UserControl
{
    internal readonly KeyBindListViewModel m_viewModel;

    public KeyBindListControl()
    {
        InitializeComponent();
        m_viewModel = new() { KeyBindEdit = KeyBindEdit.m_viewModel };
        DataContext = m_viewModel;
    }
}
