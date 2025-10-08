using Ion.Core;
using Ion.Reflect;

namespace Ion.Controls;

/// <summary>See <see cref="StyleKeys"/>.</summary>
public sealed class StyleExtension : UriExtension
{
    public StyleKeys Key { set => RelativePath = AppResources.FormatStyle.F(value); }

    public StyleExtension() : base() => Assembly = AssemblyProject.WPF;
}