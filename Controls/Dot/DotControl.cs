using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ion.Controls;

public class DotControl() : Control()
{
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(DotControl), new FrameworkPropertyMetadata(null));
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(DotControl), new FrameworkPropertyMetadata(Stretch.Fill));
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(DockControlType), typeof(DotControl), new FrameworkPropertyMetadata(DockControlType.Dot3));
    public DockControlType Type
    {
        get => (DockControlType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
}