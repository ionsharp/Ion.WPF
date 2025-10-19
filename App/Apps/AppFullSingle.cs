using System.Collections.Generic;

namespace Ion.Core;

public abstract class AppFullSingle() : AppFull(), IAppSingle
{
    public event AppReloadedEventHandler Reloaded;

    new public static AppFullSingle Current => AppFull.Current as AppFullSingle;

    public virtual void OnReloaded(IList<string> arguments)
    {
        arguments.IfNotNull(i => i.Count > 0, i => i.RemoveAt(0));
        Reloaded?.Invoke(this, new AppReloadedEventArgs(arguments));
    }
}