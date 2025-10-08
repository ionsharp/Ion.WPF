using System.Collections.Generic;

namespace Ion.Core;

public interface IAppLinkResources
{
    object GetValue(string key);

    IEnumerable<object> GetValues();
}