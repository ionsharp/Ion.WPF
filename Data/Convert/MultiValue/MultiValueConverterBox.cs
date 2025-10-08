using System;
using System.Globalization;
using System.Windows.Data;

namespace Ion.Data;

/// <inheritdoc/>
public class MultiValueConverterBox<T> : MultiValueConverter<object> where T : IValueConverter
{
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length > 0)
        {
            if (values[0] is object value)
            {
                //Assume last value decides localization and make it parameter
                parameter = values[^1];
                return ValueConverter.Cache[typeof(T)].Convert(value, targetType, parameter, culture);
            }
        }
        return Binding.DoNothing;
    }
}