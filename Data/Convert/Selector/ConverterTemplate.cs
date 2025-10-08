using System;
using System.Windows;

namespace Ion.Data;

public class ConverterTemplate() : DependencyObject()
{
    public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register(nameof(Converter), typeof(Type), typeof(ConverterTemplate), new FrameworkPropertyMetadata(null));
    public Type Converter
    {
        get => (Type)GetValue(ConverterProperty);
        set => SetValue(ConverterProperty, value);
    }

    public static readonly DependencyProperty ConverterParameterProperty = DependencyProperty.Register(nameof(ConverterParameter), typeof(object), typeof(ConverterTemplate), new FrameworkPropertyMetadata(null));
    public object ConverterParameter
    {
        get => (object)GetValue(ConverterParameterProperty);
        set => SetValue(ConverterParameterProperty, value);
    }

    public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(nameof(DataType), typeof(object), typeof(ConverterTemplate), new FrameworkPropertyMetadata(null));
    public object DataType
    {
        get => GetValue(DataTypeProperty);
        set => SetValue(DataTypeProperty, value);
    }
}