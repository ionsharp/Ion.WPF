using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class BinaryControl() : Control()
{
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(BinaryControl), new FrameworkPropertyMetadata(null));
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentTemplate1Property = DependencyProperty.Register(nameof(ContentTemplate1), typeof(DataTemplate), typeof(BinaryControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ContentTemplate1
    {
        get => (DataTemplate)GetValue(ContentTemplate1Property);
        set => SetValue(ContentTemplate1Property, value);
    }

    public static readonly DependencyProperty ContentTemplate2Property = DependencyProperty.Register(nameof(ContentTemplate2), typeof(DataTemplate), typeof(BinaryControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ContentTemplate2
    {
        get => (DataTemplate)GetValue(ContentTemplate2Property);
        set => SetValue(ContentTemplate2Property, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orient), typeof(BinaryControl), new FrameworkPropertyMetadata(Orient.Horizontal));
    public Orient Orientation
    {
        get => (Orient)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty SideProperty = DependencyProperty.Register(nameof(Side), typeof(Sides), typeof(BinaryControl), new FrameworkPropertyMetadata(Sides.LeftOrTop));
    public Sides Side
    {
        get => (Sides)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public static readonly DependencyProperty SpaceProperty = DependencyProperty.Register(nameof(Space), typeof(double), typeof(BinaryControl), new FrameworkPropertyMetadata(0.0));
    public double Space
    {
        get => (double)GetValue(SpaceProperty);
        set => SetValue(SpaceProperty, value);
    }
}