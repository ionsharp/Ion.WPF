using Ion.Reflect;
using System.Reflection;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Ion.Behavior;

public class SetPropertyBehavior() : Behavior<DependencyObject>()
{
    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target), typeof(object), typeof(SetPropertyBehavior), new FrameworkPropertyMetadata(null, Update));
    public object Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty TargetPropertyNameProperty = DependencyProperty.Register(nameof(TargetPropertyName), typeof(string), typeof(SetPropertyBehavior), new FrameworkPropertyMetadata(null, Update));
    public string TargetPropertyName
    {
        get => (string)GetValue(TargetPropertyNameProperty);
        set => SetValue(TargetPropertyNameProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(SetPropertyBehavior), new FrameworkPropertyMetadata(null, Update));
    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void Update(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        if (i is SetPropertyBehavior j)
        {
            if (j.Target != null && j.TargetPropertyName != null)
                Try.Do(() => Instance.SetPropertyValue(j.Target, j.TargetPropertyName, j.Value));
        }
    }
}