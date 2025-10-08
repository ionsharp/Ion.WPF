using Ion.Reflect;
using System;
using System.Windows.Markup;

namespace Ion.Controls;

/// <see cref="AssemblyExtension"/>
#region

public abstract class AssemblyExtension : MarkupExtension
{
    public string AssemblyName { get; set; }

    private AssemblyProject assembly = AssemblyProject.Unspecified;
    public AssemblyProject Assembly
    {
        get => assembly;
        set
        {
            assembly = value;
            AssemblyName = value.GetName();
        }
    }

    protected AssemblyExtension() : this(AssemblyProject.Main) { }

    protected AssemblyExtension(AssemblyProject assembly) : base() => Assembly = assembly;
}

#endregion

/// <see cref="AssemblyCopyright"/>
#region

public sealed class AssemblyCopyright() : AssemblyExtension(AssemblyProject.Main)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).Copyright;
}

#endregion

/// <see cref="AssemblyDescription"/>
#region

public sealed class AssemblyDescription() : AssemblyExtension(AssemblyProject.Main)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).Description;
}

#endregion

/// <see cref="AssemblyFileVersion"/>
#region

public sealed class AssemblyFileVersion() : AssemblyExtension(AssemblyProject.Main)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).FileVersion.TrimZero();
}

#endregion

/// <see cref="AssemblyName"/>
#region

public class AssemblyName(AssemblyProject assembly) : AssemblyExtension(assembly)
{
    public AssemblyName() : this(AssemblyProject.Main) { }

    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).Name;
}

#endregion

/// <see cref="AssemblyNameDefault"/>
#region

public sealed class AssemblyNameDefault() : AssemblyName(AssemblyProject.WPF) { }

#endregion

/// <see cref="AssemblyProduct"/>
#region

public sealed class AssemblyProduct() : AssemblyExtension(AssemblyProject.Main)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).Product;
}

#endregion

/// <see cref="AssemblyTitle"/>
#region

public sealed class AssemblyTitle() : AssemblyExtension(AssemblyProject.Main)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).Title;
}

#endregion

/// <see cref="AssemblyVersion"/>
#region

[Obsolete("Not discoverable. Use 'AssemblyFileVersion' instead.")]
public class AssemblyVersion() : AssemblyExtension(AssemblyProject.Main)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => XAssembly.GetInfo(AssemblyName).Version.TrimZero();
}

#endregion