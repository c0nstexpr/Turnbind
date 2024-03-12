using MoreLinq;

using Turnbind.Model;

namespace Turnbind.Action;

class ProfileSubscription(string profileName) : Dictionary<InputKeys, IDisposable>, IDisposable
{
    public string ProfileName { get; } = profileName;

    public void Dispose() => this.ForEach(item => item.Value.Dispose());
}