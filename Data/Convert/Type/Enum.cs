using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Ion.Data;

public class EnumTypeConverter<T>() : TypeConverter()
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string text)
        {
            var values = Enumerable.Select(text.Split('|', StringSplitOptions.RemoveEmptyEntries), i => Enum.Parse(typeof(T), i)).Cast<Enum>();

            Enum result = null;
            foreach (Enum i in values)
                result = result is null ? i : result.AddFlag(i);

            return result;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class FilterTypeConverter() : EnumTypeConverter<Filter>() { }