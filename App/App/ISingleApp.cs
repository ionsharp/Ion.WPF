using System.Collections.Generic;

namespace Ion.Core;

public interface ISingleApp
{
    event AppReloadedEventHandler Reloaded;

    void OnReloaded(IList<string> Arguments);
}