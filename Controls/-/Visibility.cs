using System.Windows;

namespace Ion.Controls;

[Extend<Visibility>]
public static class XVisibility
{
    public static Visibility Invert(this Visibility i, Visibility j = Visibility.Collapsed)
        => i == Visibility.Visible ? j : Visibility.Visible;

    public static Visibility ToVisibility(this bool i, Visibility falseVisibility = Visibility.Collapsed)
        => i ? Visibility.Visible : falseVisibility;
}