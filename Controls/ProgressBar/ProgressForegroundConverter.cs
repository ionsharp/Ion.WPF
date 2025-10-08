using Ion.Data;
using System.Windows.Media;

namespace Ion.Controls;

[Convert<LinearGradientBrush, SolidColorBrush>]
public class ProgressForegroundConverter() : ValueConverter<SolidColorBrush, LinearGradientBrush>()
{
    private static LinearGradientBrush Convert(SolidColorBrush input)
    {
        var result = new LinearGradientBrush();

        double offset = 0;
        for (int i = 0; i < 5; i++, offset += 0.25)
        {
            _ = input.Color;

            byte alpha = 0;
            switch (i)
            {
                case 1:
                case 3:
                    alpha = System.Convert.ToByte((255.0 / 5.0) * 2.0);
                    break;
                case 2:
                    alpha = System.Convert.ToByte((255.0 / 5.0) * 3.0);
                    break;
            }

            Color color = Color.FromArgb(alpha, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            result.GradientStops.Add(new GradientStop(color, offset));
        }

        return result;
    }

    protected override ValueConverterOutput<LinearGradientBrush> Convert(ValueConverterInput<SolidColorBrush> input) => Convert(input.Value);
}