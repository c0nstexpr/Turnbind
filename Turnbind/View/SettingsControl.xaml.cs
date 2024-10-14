using System.Windows.Controls;

using Turnbind.ViewModel;

namespace Turnbind.View;

sealed partial class SettingsControl : UserControl, IDisposable
{
    internal readonly SettingViewModel m_viewModel;

    public SettingsControl()
    {
        InitializeComponent();

        m_viewModel = new()
        {
            Profile = Profile.m_viewModel,
            Binds = Binds.m_viewModel
        };

        DataContext = m_viewModel;
    }

    public void Dispose()
    {
        m_viewModel.Dispose();
        Profile.Dispose();
        Binds.Dispose();
    }
}
