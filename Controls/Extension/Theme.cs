using System.Windows;

namespace Ion.Controls;

public sealed class ThemeExtension() : DynamicResourceExtension()
{
    public Themes Key { set => ResourceKey = $"{value}"; }

    public ThemeExtension(Themes key) : this() => Key = key;
}