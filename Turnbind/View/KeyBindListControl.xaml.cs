using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class KeyBindListControl : UserControl, IDisposable
{
    internal readonly KeyBindListViewModel m_viewModel;

    public KeyBindListControl()
    {
        InitializeComponent();
        m_viewModel = new() { KeyBindEdit = KeyBindEdit.m_viewModel };
        DataContext = m_viewModel;
    }

    public void Dispose() => KeyBindEdit.Dispose();
}
