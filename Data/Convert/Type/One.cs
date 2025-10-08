using Ion.Numeral;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Ion.Data;

public class OneTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string actualValue)
        {
            if (actualValue.Length > 0)
            {
                if (actualValue[^1] == '%')
                {
                    actualValue = actualValue[..^1];
                    return (Double1)(double.Parse(actualValue) / 100d);
                }
                return (Double1)double.Parse(actualValue);
            }
        }
        throw new InvalidOperationException();
    }
}