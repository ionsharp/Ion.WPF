using System;

namespace Ion.Reflect;

public enum AssemblyProject
{
    /// <summary>
    /// Ion
    /// </summary>
    Core,
    /// <summary>
    /// Ion.Color
    /// </summary>
    Color,
    /// <summary>
    /// <see cref="AssemblySource.Entry"/>
    /// </summary>
    Main,
    /// <summary>
    /// Ion.WPF
    /// </summary>
    WPF,
    [Hide]
    Unspecified
}

[Extend<AssemblyProject>]
public static class XAssemblyProject
{
    public static string GetName(this AssemblyProject type)
    {
        return type switch
        {
            AssemblyProject.Color
                => XAssembly.Name + ".Color",
            AssemblyProject.WPF
                => XAssembly.Name + ".WPF",
            AssemblyProject.Main
                => XAssembly.Entry.GetName().Name,
            AssemblyProject.Core
                => XAssembly.Name,
            _ => throw new NotSupportedException()
        };
    }
}