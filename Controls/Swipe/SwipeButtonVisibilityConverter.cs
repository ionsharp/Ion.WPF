using Ion.Data;
using System;
using System.Globalization;
using System.Windows;

namespace Ion.Controls;

public class SwipeButtonVisibilityConverter : MultiValueConverter<Visibility>
{
    public SwipeButtonVisibilityConverter() : base() { }

    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 3)
        {
            if (values[0] is bool a)
            {
                if (values[1] is bool isAnimating)
                {
                    if (values[2] is bool isSwiping)
                        return (a && (isAnimating || isSwiping)).ToVisibility();
                }
            }
        }
        return Visibility.Collapsed;
    }
}