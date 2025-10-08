using Ion.Imaging;
using Ion.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ion.Reflect;

public class CreateFromObject() : object(), ICreateFromObject
{
    private static WriteableBitmap Clone(System.Drawing.Bitmap i) => XWriteableBitmap.Clone(XWriteableBitmap.Convert(i));

    private static System.Drawing.Color Clone(System.Drawing.Color i) => i;

    private static System.Windows.Media.Color Clone(System.Windows.Media.Color i) => i;

    private static ColorViewModel Clone(ColorViewModel i) => Instance.CloneDeep(i);

    private static LinearGradientBrush Clone(LinearGradientBrush i) => new(i.GradientStops);

    private static RadialGradientBrush Clone(RadialGradientBrush i) => new(i.GradientStops);

    private static SolidColorBrush Clone(SolidColorBrush i) => new(i.Color);

    private static object Clone(System.Windows.Media.Brush _) => default;

    private static WriteableBitmap Clone(WriteableBitmap i) => XWriteableBitmap.Clone(i);

    public object Create(object i)
    {
        if (i is System.Drawing.Bitmap)
            return Clone(i.To<System.Drawing.Bitmap>());

        if (i is System.Drawing.Color)
            return Clone(i.To<System.Drawing.Color>());

        if (i is System.Windows.Media.Color)
            return Clone(i.To<System.Windows.Media.Color>());

        if (i is ColorViewModel)
            return Clone(i.To<ColorViewModel>());

        if (i is LinearGradientBrush)
            return Clone(i.To<LinearGradientBrush>());

        if (i is RadialGradientBrush)
            return Clone(i.To<RadialGradientBrush>());

        if (i is SolidColorBrush)
            return Clone(i.To<SolidColorBrush>());

        if (i is System.Windows.Media.Brush)
            return Clone(i.To<System.Windows.Media.Brush>());

        if (i is WriteableBitmap)
            return Clone(i.To<WriteableBitmap>());

        return Instance.CloneDeep(i, this);
    }
}