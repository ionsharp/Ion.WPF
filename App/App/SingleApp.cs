using System.Collections.Generic;

namespace Ion.Core;

public abstract class SingleApp() : App(), ISingleApp
{
    public event AppReloadedEventHandler Reloaded;

    public virtual void OnReloaded(IList<string> arguments)
    {
        arguments.IfNotNull(i => i.Count > 0, i => i.RemoveAt(0));
        Reloaded?.Invoke(this, new AppReloadedEventArgs(arguments));
    }
}