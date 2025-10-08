using System.Windows.Media;

namespace Ion.Media;

[Extend<Brush>]
public static class XBrush
{
    /// <summary>
    /// Gets a new <see cref="Brush"/> from the given <see cref="Brush"/> (not deep copy).
    /// </summary>
    public static Brush Clone(this Brush input)
    {
        if (input is LinearGradientBrush a)
        {
            return new LinearGradientBrush(a.GradientStops)
            {
                StartPoint
                    = a.StartPoint,
                EndPoint
                    = a.EndPoint,
                MappingMode
                    = a.MappingMode,
                Opacity
                    = a.Opacity,
                ColorInterpolationMode
                    = a.ColorInterpolationMode,
                SpreadMethod
                    = a.SpreadMethod,
                RelativeTransform
                    = a.RelativeTransform,
                Transform
                    = a.Transform
            };
        }
        else if (input is RadialGradientBrush b)
        {
            return new RadialGradientBrush(b.GradientStops)
            {
                MappingMode
                    = b.MappingMode,
                Opacity
                    = b.Opacity,
                ColorInterpolationMode
                    = b.ColorInterpolationMode,
                SpreadMethod
                    = b.SpreadMethod,
                RelativeTransform
                    = b.RelativeTransform,
                Transform
                    = b.Transform
            };
        }
        return default;
    }
}