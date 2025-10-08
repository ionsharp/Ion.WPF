using Ion.Colors;
using Ion.Imaging;
using System.Windows;
using System.Windows.Media;

namespace Ion.Media;

[Extend<LinearGradientBrush>]
public static class XLinearGradientBrush
{
    public static void Convert(this Gradient gradient, out LinearGradientBrush result)
    {
        result = new LinearGradientBrush()
        {
            EndPoint = new Point(Gradient.Horizontal.X2, Gradient.Horizontal.Y2),
            StartPoint = new Point(Gradient.Horizontal.X1, Gradient.Horizontal.Y1),
            Opacity = 1,
        };
        foreach (var i in gradient.Steps)
            result.GradientStops.Add(new GradientStop(XColor.Convert(i.Color), i.Offset));
    }

    public static void Convert(this LinearGradientBrush gradient, out Gradient result)
    {
        result = default;
    }
}