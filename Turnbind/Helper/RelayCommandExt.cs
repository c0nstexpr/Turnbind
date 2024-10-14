using CommunityToolkit.Mvvm.Input;

namespace Turnbind.Helper;

public static class RelayCommandExt
{
    public static readonly RelayCommand DefaultCommand = new(static () => { }, static () => false);
}
