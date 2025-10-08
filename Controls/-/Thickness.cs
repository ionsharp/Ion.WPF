using System.Windows;

namespace Ion.Controls;

[Extend<Thickness>]
public static class XThickness
{
    public static double GetHeight(this Thickness i) => i.Top + i.Bottom;

    public static double GetWidth(this Thickness i) => i.Left + i.Right;

    public static Thickness Invert(this Thickness i) => new(-i.Left, -i.Top, -i.Right, -i.Bottom);
}