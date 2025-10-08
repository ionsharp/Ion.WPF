using System.Windows;

namespace Ion.Controls;

/// <summary>See <see cref="Constants"/>.</summary>
public sealed class Constant(Constants key) : DynamicResourceExtension($"{key}")
{
    public Constants Key { set => ResourceKey = $"{value}"; }

    public Constant() : this(default) { }
}