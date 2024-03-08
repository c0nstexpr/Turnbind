using System.Windows;
using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

public partial class TurnBindEditControl : UserControl
{
    internal BindEditViewModel m_viewModel = new();

    public TurnBindEditControl()
    {
        DataContext = m_viewModel;
        InitializeComponent();
    }
}
