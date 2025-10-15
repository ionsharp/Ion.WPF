using Ion.Reflect;
using System;

namespace Ion.Core;

/// <summary>An independent component that extends functionality of an application.</summary>
public interface IAppLink
{
    /// <see cref="Region.Event"/>

    event EventHandler<EventArgs> Enabled;

    event EventHandler<EventArgs> Disabled;

    /// <see cref="Region.Property"/>

    AssemblyContext AssemblyContext { get; set; }

    Type TargetType { get; }

    ///

    string Author { get; }

    string Description { get; }

    string FilePath { get; set; }

    string Icon { get; }

    bool IsEnabled { get; set; }

    string Name { get; }

    string Uri { get; }

    Version Version { get; }

    /// <see cref="Region.Method"/>

    void OnEnabled();

    void OnDisabled();
}