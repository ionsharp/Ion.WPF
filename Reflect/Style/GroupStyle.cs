using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class GroupStyleAttribute() : Attribute()
{
    public object Icon { get; set; }

    public int Index { get; set; }

    public object Name { get; set; }
}