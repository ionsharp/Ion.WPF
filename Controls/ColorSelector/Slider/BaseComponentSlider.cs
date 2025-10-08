using Ion.Numeral;
using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

public class BaseComponentSlider : ColorSelector
{
    public static readonly DependencyProperty ArrowForegroundProperty = DependencyProperty.Register(nameof(ArrowForeground), typeof(Brush), typeof(BaseComponentSlider), new FrameworkPropertyMetadata(Brushes.Black));
    public Brush ArrowForeground
    {
        get => (Brush)GetValue(ArrowForegroundProperty);
        set => SetValue(ArrowForegroundProperty, value);
    }

    private static readonly DependencyPropertyKey ArrowPositionKey = DependencyProperty.RegisterReadOnly(nameof(ArrowPosition), typeof(Vector2M<double>), typeof(BaseComponentSlider), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ArrowPositionProperty = ArrowPositionKey.DependencyProperty;
    public Vector2M<double> ArrowPosition
    {
        get => (Vector2M<double>)GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionKey, value);
    }

    public static readonly DependencyProperty ArrowTemplateProperty = DependencyProperty.Register(nameof(ArrowTemplate), typeof(DataTemplate), typeof(BaseComponentSlider), new FrameworkPropertyMetadata(null));
    public DataTemplate ArrowTemplate
    {
        get => (DataTemplate)GetValue(ArrowTemplateProperty);
        set => SetValue(ArrowTemplateProperty, value);
    }

    public BaseComponentSlider() : base()
    {
        ArrowPosition = new Vector2M<double>(0, -8);
    }
}