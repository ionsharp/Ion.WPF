using System.Collections.Generic;

namespace Ion.Core;

public interface IAppSingle
{
    event AppReloadedEventHandler Reloaded;

    void OnReloaded(IList<string> Arguments);
}