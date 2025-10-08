using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class BarGraph : ItemsControl
{
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(BarGraph), new FrameworkPropertyMetadata(0.0));
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public BarGraph() : base() { }

    protected override DependencyObject GetContainerForItemOverride() => new BarGraphItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is BarGraphItem;
}