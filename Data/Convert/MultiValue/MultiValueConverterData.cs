using System;
using System.Globalization;

namespace Ion.Data;

public class MultiValueConverterData(object[] values, Type targetType, object parameter, CultureInfo culture)
{
    public readonly CultureInfo Culture = culture;

    public readonly object[] Values = values;

    public readonly object Parameter = parameter;

    public readonly Type TargetType = targetType;
}