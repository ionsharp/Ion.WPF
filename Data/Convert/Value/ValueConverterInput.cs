using System;
using System.Globalization;

namespace Ion.Data;

public class ValueConverterInput<T>
{
    public readonly int Parameter;

    public T Value => ActualValue is T i ? i : default;

    public readonly object ActualParameter;

    public readonly object ActualValue;

    public CultureInfo Culture { get; set; }

    public Type TargetType { get; set; }

    public ArgumentOutOfRangeException InvalidParameter => new(nameof(Parameter));

    public ValueConverterInput(object value, object parameter)
    {
        ActualValue = value;
        ActualParameter = parameter;
        int.TryParse(ActualParameter?.ToString(), out Parameter);
    }
}