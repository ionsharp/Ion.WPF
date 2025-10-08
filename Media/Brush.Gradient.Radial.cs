using Ion.Colors;
using Ion.Imaging;
using System.Windows.Media;

namespace Ion.Media;

[Extend<RadialGradientBrush>]
public static class XRadialGradientBrush
{
    public static void Convert(this Gradient gradient, out RadialGradientBrush result)
    {
        result = new RadialGradientBrush()
        {
            RadiusX = 0.5,
            RadiusY = 0.5,
            Opacity = 1,
        };
        foreach (var i in gradient.Steps)
            result.GradientStops.Add(new GradientStop(XColor.Convert(i.Color), i.Offset));
    }

    public static void Convert(this RadialGradientBrush gradient, out Gradient result)
    {
        result = default;
    }
}