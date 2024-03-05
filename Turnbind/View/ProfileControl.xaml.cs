using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

    void ProfileNameListDoubleClick(object sender, MouseButtonEventArgs e) => m_viewModel.OnProfileNameListSelected();

    void AddButtonClick(object sender, RoutedEventArgs e) => m_viewModel.OnAddProfileName();

    void RemoveButtonClick(object sender, RoutedEventArgs e) => m_viewModel.OnRemoveProfileName();
}
