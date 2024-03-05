using System.Windows;
using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

public partial class TurnBindEditControl : UserControl
{
    internal TurnBindEditViewModel m_viewModel = new();

    public TurnBindEditControl()
    {
        DataContext = m_viewModel;
        InitializeComponent();
    }

    void AddButtonClick(object sender, RoutedEventArgs e) => m_viewModel.OnAdd();

    void ModifyButtonClick(object sender, RoutedEventArgs e) => m_viewModel.OnModify();

    void RemoveButtonClick(object sender, RoutedEventArgs e) => m_viewModel.OnRemove();

    void BindingKeysTextBoxGotFocus(object sender, RoutedEventArgs e) => m_viewModel.OnBindingKeysFocus();

    void BindingKeysTextBoxLostFocus(object sender, RoutedEventArgs e) => m_viewModel.OnBindingKeysLostFocus();
}
