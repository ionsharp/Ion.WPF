using Ion.Reflect;
using System;

namespace Ion.Controls;

public class UriExtension(string relativePath, AssemblyProject assembly) : AssemblyExtension(assembly)
{
    public string RelativePath { get; set; } = relativePath;

    public UriExtension() : this(null) { }

    public UriExtension(string relativePath) : this(relativePath, AssemblyProject.Main) { }

    public UriExtension(string assemblyName, string relativePath) : this(relativePath)
        => AssemblyName = assemblyName;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (AssemblyName is null)
            return new Uri(RelativePath, UriKind.Relative);

        return Resource.GetUri(AssemblyName, RelativePath);
    }
}