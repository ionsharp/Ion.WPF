using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
public sealed class TabViewAttribute() : Attribute()
{
    public View View { get; set; } = View.None;
}