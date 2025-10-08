using Ion.Core;
using Ion.Reflect;
using System;

namespace Ion.Controls;

public class ReadExtension(string relativePath, AssemblyProject assembly = AssemblyProject.Main) : UriExtension(relativePath, assembly)
{
    public override object ProvideValue(IServiceProvider serviceProvider) => AppResources.GetText(RelativePath, Assembly);
}