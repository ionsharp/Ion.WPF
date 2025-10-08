using Ion.Imaging;
using Ion.Media;
using Ion.Numeral;
using System.Windows.Media;

namespace Ion.Media;

[Extend<SolidColorBrush>]
public static class XSolidColorBrush
{
    public static void Convert(this SolidColorBrush input, out ByteVector4 result) => input.Color.Convert(out result);

    public static SolidColorBrush Convert(ByteVector4 input) => new(XColor.Convert(input));
}