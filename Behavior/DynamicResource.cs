using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Ion.Behavior;

public class DynamicResourceBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(object), typeof(DynamicResourceBehavior), new FrameworkPropertyMetadata(null));
    public object Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(nameof(Property), typeof(DependencyProperty), typeof(DynamicResourceBehavior), new FrameworkPropertyMetadata(null));
    public DependencyProperty Property
    {
        get => (DependencyProperty)GetValue(PropertyProperty);
        set => SetValue(PropertyProperty, value);
    }

    protected override void OnAttached()
    {
        if (AssociatedObject is not null)
        {
            if (Property is not null)
            {
                if (Key is not null)
                    AssociatedObject.SetResourceReference(Property, Key);
            }
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        OnAttached();
    }
}