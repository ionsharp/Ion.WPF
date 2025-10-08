using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class BarGraphItem : ContentControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(BarGraphItem), new FrameworkPropertyMetadata(0.0));
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public BarGraphItem() : base() { }
}