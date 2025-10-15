using System;
using System.Collections.Generic;
using System.Windows;

namespace Ion.Core;

public delegate void AppLoadedEventHandler(IApp input, AppLoadedEventArgs e);

/// <inheritdoc cref="StartupEventArgs"/>
/// <remarks>Wraps <see cref="StartupEventArgs"/>.</remarks>
public class AppLoadedEventArgs(StartupEventArgs e) : EventArgs()
{
    public StartupEventArgs Source { get; } = e;

    public IList<string> Arguments { get; } = e.Args;
}