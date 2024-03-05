using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Linq;

using Turnbind.Model;

namespace Turnbind.ViewModel;

partial class TurnBindViewModel : ObservableObject
{
    //readonly BindingEditViewModel m_editViewModel = new();

    //readonly BindingListView m_listView = new();

    //ObservableCollection<BindingViewModel> m_binds => m_listView.Binds;

    //Settings m_settings = new();

    //public Settings Settings
    //{
    //    get => m_settings;

    //    set
    //    {
    //        SetProperty(ref m_settings, value);
    //        SetBinds();
    //    }
    //}

    //public TurnBindViewModel() => SetBinds();

    //void SetBinds() => m_listView.SetBinds(m_settings.Profile);

    //public bool Modify()
    //{
    //    TurnSetting dummyBind = new() { Keys = m_editViewModel.Binding.Keys };

    //    if (m_focusedBinding == null)
    //    {
    //        if (m_binds.Contains(dummyBind))
    //            return false;

    //        BindingViewModel newBind = new()
    //        {
    //            Dir = TurnDirection,
    //            PixelPerSec = PixelPerSec,
    //            Keys = [.. m_newBindingKeys]
    //        };

    //        Binds.Add(newBind);
    //    }
    //    else
    //    {
    //        m_binds.Remove(dummyBind);

    //        m_focusedBinding.Keys = [.. m_newBindingKeys];
    //        m_focusedBinding.Dir = TurnDirection;
    //        m_focusedBinding.PixelPerSec = PixelPerSec;

    //        m_binds.Add(m_focusedBinding.Binding);
    //    }

    //    Settings.Save();

    //    return true;
    //}

    //public void Remove()
    //{
    //    Debug.Assert(RemoveButtonEnabled);
    //    Binds.RemoveAt(FocusedBindingIndex);
    //    Settings.Save();
    //}


    //[ObservableProperty]
    //string m_modifyButtonContent = "Add";

    //[ObservableProperty]
    //bool m_removeButtonEnabled;

    //static TurnSetting GetElement(IList items) => items.Cast<BindingViewModel>().First().Binding;
}
