using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class BindsControl : UserControl, IDisposable
{
    internal readonly BindsViewModel m_viewModel;

    public BindsControl()
    {
        InitializeComponent();
        m_viewModel = new() { BindEdit = BindEdit.m_viewModel };
        DataContext = m_viewModel;
    }

    public void Dispose() => BindEdit.Dispose();
}
