using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

public class LineElement : FrameworkElement
{
    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(LineElement), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(LineElement), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orient), typeof(LineElement), new FrameworkPropertyMetadata(Orient.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender));
    public Orient Orientation
    {
        get => (Orient)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public LineElement() : base() { }

    protected override void OnRender(DrawingContext context)
    {
        base.OnRender(context);
        context.DrawRectangle(Stroke, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
    }
}