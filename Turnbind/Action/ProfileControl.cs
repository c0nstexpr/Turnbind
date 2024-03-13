using MoreLinq;

using Turnbind.Model;

namespace Turnbind.Action;

class ProfileControl(string profileName) : IDisposable
{
    public string ProfileName { get; } = profileName;

    readonly Dictionary<InputKeys, BindControl> m_binds = [];

    public void Add(InputKeys keys, TurnSetting setting)
    {
        BindControl control = new(keys, setting);
        m_binds.Add(keys, control);

        if (Active) control.Enable();
    }

    public void Remove(InputKeys keys) => m_binds.Remove(keys);

    public void Contains(InputKeys keys) => m_binds.ContainsKey(keys);

    public bool Active { get; private set; } = false;

    public void Enable(ProcessWindowAction windowAction, InputAction inputAction, TurnAction turnAction)
    {
        m_binds.Values.ForEach(item => item.Enable(windowAction, inputAction, turnAction));
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