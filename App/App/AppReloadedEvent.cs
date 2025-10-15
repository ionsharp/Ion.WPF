using System;
using System.Collections.Generic;

namespace Ion.Core;

public delegate void AppReloadedEventHandler(IAppSingle sender, AppReloadedEventArgs e);

public class AppReloadedEventArgs(IEnumerable<string> arguments) : EventArgs()
{
    public readonly List<string> Arguments = new(arguments);
}