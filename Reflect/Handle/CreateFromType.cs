using System;
using System.Windows.Media;

namespace Ion.Reflect;

public class CreateFromType : ICreateFromType
{
    public CreateFromType() : base() { }

    object ICreateFrom<Type>.Create(Type value) => Create(value);

    public static object Create(Type value)
    {
        if (value == typeof(System.Windows.Media.Color))
            return System.Windows.Media.Colors.Transparent;

        if (value == typeof(SolidColorBrush))
            return System.Windows.Media.Brushes.Transparent;

        if (value == typeof(string))
            return "";

        return value.Create<object>();
    }
}