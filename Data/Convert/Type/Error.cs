using Ion.Analysis;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Ion.Data;

public class ErrorTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(Error))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is Error error)
            return error;

        return base.ConvertFrom(context, culture, value);
    }
}