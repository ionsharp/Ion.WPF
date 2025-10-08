using System.Windows;

namespace Ion.Controls;

public class Setter() : DependencyObject()
{
    /// <see cref="Property"/>
    #region

    public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(nameof(Property), typeof(DependencyProperty), typeof(Setter), new FrameworkPropertyMetadata(null));
    public DependencyProperty Property
    {
        get => (DependencyProperty)GetValue(PropertyProperty);
        set => SetValue(PropertyProperty, value);
    }

    #endregion

    /// <see cref="Value"/>
    #region

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Setter), new FrameworkPropertyMetadata(null));
    public object Value
    {
        get => (object)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion
}