using Ion.Reflect;
using System;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, Inherited = true)]
public sealed class ImageAttribute() : Attribute()
{
    /// <see cref="Region.Property"/>
    #region

    public object Name { get; set; }

    public AssemblyProject NameAssembly { get; set; } = AssemblyProject.Main;

    public object Checked { get; set; }

    public AssemblyProject CheckedAssembly { get; set; } = AssemblyProject.Main;

    public object Color { get; set; }

    public Type ColorType { get; set; }

    public bool Mask { get; set; } = true;

    public object Template { get; set; }

    public Type TemplateType { get; set; }

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public ImageAttribute(Images name) : this()
    {
        Name = name;
        NameAssembly = AssemblyProject.WPF;
    }

    public ImageAttribute(string name, AssemblyProject assembly = AssemblyProject.Main) : this()
    {
        Name = name;
        NameAssembly = assembly;
    }

    #endregion
}