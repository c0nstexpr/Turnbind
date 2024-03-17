using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class KeyBindsControl : UserControl, IDisposable
{
    internal readonly KeyBindsViewModel m_viewModel;

    public KeyBindsControl()
    {
        InitializeComponent();

        m_viewModel = new()
        {
            Profile = Profile.m_viewModel,
            KeyBindList = KeyBindList.m_viewModel
        };

        DataContext = m_viewModel;
    }

    public void Dispose()
    {
        m_viewModel.Dispose();
        Profile.Dispose();
        KeyBindList.Dispose();
    }
}
