﻿using CommunityToolkit.Mvvm.ComponentModel;

using Turnbind.Model;
using Turnbind.View;

namespace Turnbind.ViewModel;

partial class KeyBindViewModel : ObservableObject
{
    public static TurnDirection[] DirectionValues { get; } = Enum.GetValues<TurnDirection>();

    InputKeys m_keys = new();

    public InputKeys Keys
    {
        get => m_keys;

        set
        {
            SetProperty(ref m_keys, value);
            OnPropertyChanged(nameof(KeysString));
        }
    }

    public string KeysString => m_keys.ToKeyString();

    [ObservableProperty]
    TurnSettingViewModel m_turnSetting = new();
}
