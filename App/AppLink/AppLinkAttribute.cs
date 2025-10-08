using System;

namespace Ion.Core;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AppLinkAttribute : Attribute
{
    public string Description { get; set; }

    public object Icon { get; set; }

    public string Name { get; set; }

    public string Uri { get; set; }

    public Version Version { get; set; } = new(1, 0, 0, 0);
}