using Ion.Reflect;
using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
public sealed class TabStyleAttribute() : Attribute()
{
    public string Description { get; set; }

    public Fill Fill { get; set; } = Fill.None;

    public bool Group { get; set; } = true;

    public object Image { get; set; }

    public AssemblyProject ImageSource { get; set; } = AssemblyProject.WPF;

    public string Name { get; set; }
}