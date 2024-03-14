﻿using MoreLinq;

using Turnbind.Model;

namespace Turnbind.Action;

class ProfileControl(string profileName) : IDisposable
{
    public string ProfileName { get; } = profileName;

    readonly Dictionary<InputKeys, BindControl> m_binds = [];

    public void Add(InputKeys keys, BindControl control) => m_binds.Add(keys, control);

    public void Remove(InputKeys keys)
    {
        if (!m_binds.TryGetValue(keys, out var control)) return;
        control.Dispose();
        m_binds.Remove(keys);
    }

    public void Contains(InputKeys keys) => m_binds.ContainsKey(keys);

    public bool Active { get; private set; } = false;

    public void Enable()
    {
        m_binds.Values.ForEach(item => item.Enable());
        Active = true;
    }

    public void Disable()
    {
        m_binds.Values.ForEach(item => item.Disable());
        Active = false;
    }

    public void Dispose() =>
        m_binds.Values.ForEach(item => item.Dispose());
}