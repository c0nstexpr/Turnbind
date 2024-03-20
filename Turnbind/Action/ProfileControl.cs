using MoreLinq;

using Turnbind.Model;

namespace Turnbind.Action;

class ProfileControl(string profileName) : IDisposable
{
    public string ProfileName { get; } = profileName;

    readonly Dictionary<InputKeys, BindControl> m_binds = [];

    public void Add(InputKeys keys, TurnSetting turnSetting)
    {
        if (m_binds.ContainsKey(keys)) return;

        BindControl control = new()
        {
            Keys = keys,
            Setting = turnSetting
        };

        m_binds.Add(keys, control);

        if (Enable) control.Enable = true;
    }

    public void Remove(InputKeys keys)
    {
        if (!m_binds.TryGetValue(keys, out var control)) return;
        control.Dispose();
        m_binds.Remove(keys);
    }

    public void Update(InputKeys keys, TurnSetting turnSetting)
    {
        if (!m_binds.TryGetValue(keys, out var control)) return;
        control.Setting = turnSetting;
    }

    public void Contains(InputKeys keys) => m_binds.ContainsKey(keys);

    bool m_enable = false;

    public bool Enable
    {
        get => m_enable;

        set
        {
            m_enable = value;
            m_binds.Values.ForEach(item => item.Enable = value);
        }
    }

    public void Dispose() =>
        m_binds.Values.ForEach(item => item.Dispose());

    public void Clear()
    {
        m_binds.Values.ForEach(item => item.Dispose());
        m_binds.Clear();
    }
}