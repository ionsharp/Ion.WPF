using Ion.Data;
using Ion.Numeral;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

/// <summary>Specifies a single element that rotates.</summary>
public class RotateElement() : Element()
{
    private RotateTransform Transform => Child?.RenderTransform as RotateTransform;

    public static readonly DependencyProperty RotateProperty = DependencyProperty.Register(nameof(Rotate), typeof(bool), typeof(RotateElement), new FrameworkPropertyMetadata(false));
    public bool Rotate
    {
        get => (bool)GetValue(RotateProperty);
        set => SetValue(RotateProperty, value);
    }

    public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(nameof(Rotation), typeof(Range<Angle>), typeof(RotateElement), new FrameworkPropertyMetadata(default(Range<Angle>), FrameworkPropertyMetadataOptions.AffectsRender));
    [TypeConverter(typeof(DegreeRangeTypeConverter))]
    public Range<Angle> Rotation
    {
        get => (Range<Angle>)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public static readonly DependencyProperty RotationScaleProperty = DependencyProperty.Register(nameof(RotationScale), typeof(double), typeof(RotateElement), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
    public double RotationScale
    {
        get => (double)GetValue(RotationScaleProperty);
        set => SetValue(RotationScaleProperty, value);
    }

    ///

    protected override int VisualChildrenCount => GetValue(ChildProperty) is not null ? 1 : 0;

    protected override Visual GetVisualChild(int index) => (UIElement)GetValue(ChildProperty);

    ///

    protected override void OnChildChanged(ValueChange<Visual> input)
    {
        base.OnChildChanged(input);
        input.NewValue.SetCurrentValue(RenderTransformProperty, new RotateTransform() { Angle = Rotation.Minimum });
        input.NewValue.SetCurrentValue(RenderTransformOriginProperty,
            new Point(0.5, 0.5));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        Transform.IfNotNull(i => i.Angle = Rotation.Minimum + ((Rotation.Maximum - Rotation.Minimum) * RotationScale));
    }
}