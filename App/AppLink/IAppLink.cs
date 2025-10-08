using Ion.Reflect;
using System;

namespace Ion.Core;

/// <summary>An independent component that extends functionality of an application.</summary>
public interface IAppLink
{
    /// <see cref="Region.Event"/>
    #region

    event EventHandler<EventArgs> Enabled;

    event EventHandler<EventArgs> Disabled;

    #endregion

    /// <see cref="Region.Property"/>
    #region

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

    #endregion

    /// <see cref="Region.Method"/>
    #region

    void OnEnabled();

    void OnDisabled();

    #endregion
}