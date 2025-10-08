using Ion.Colors;
using Ion.Numeral;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

[TemplatePart(Name = nameof(PART_Canvas), Type = typeof(Canvas))]
public class ColorSelector() : Control()
{
    private Canvas PART_Canvas;

    public static readonly DependencyProperty DepthProperty = DependencyProperty.Register(nameof(Depth), typeof(double), typeof(ColorSelector), new FrameworkPropertyMetadata(0.0));
    public double Depth
    {
        get => (double)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(Type), typeof(ColorSelector), new FrameworkPropertyMetadata(typeof(HSB)));
    public Type Model
    {
        get => (Type)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public static readonly DependencyProperty ProfileProperty = DependencyProperty.Register(nameof(Profile), typeof(ColorProfile), typeof(ColorSelector), new FrameworkPropertyMetadata(ColorProfile.Default));
    public ColorProfile Profile
    {
        get => (ColorProfile)GetValue(ProfileProperty);
        set => SetValue(ProfileProperty, value);
    }

    ///

    private Vector2<Double1> CoerceDepth(Vector2<Double1> a)
    {
        if (Depth > 0)
        {
            var c = new Vector2(a.X, a.Y) * Depth;
            var b = c.Round() / Depth;
            a = new(b.X, b.Y);
        }
        return a;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (Mouse.LeftButton == MouseButtonState.Pressed)
        {
            var point = Normalize(e.GetPosition(this));
            point = CoerceDepth(point);

            OnMouseChanged(point);
            CaptureMouse();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (Mouse.LeftButton == MouseButtonState.Pressed)
        {
            var point = Normalize(e.GetPosition(this));
            point = CoerceDepth(point);

            OnMouseChanged(point);
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (Mouse.LeftButton == MouseButtonState.Released)
            ReleaseMouseCapture();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        Mark();
    }

    ///

    /// <summary>Gets a <see cref="Point"/> in range [0, 1].</summary>
    protected Vector2<Double1> Normalize(Point input)
    {
        input = input.Coerce(new Point(ActualWidth, ActualHeight), new Point(0, 0));
        input = new Point(input.X, ActualHeight - input.Y);
        input = new Point(input.X / ActualWidth, input.Y / ActualHeight);
        return new Vector2<Double1>(input.X, input.Y);
    }

    ///

    protected virtual void Mark() { }

    protected virtual void OnMouseChanged(Vector2<Double1> input) { }

    ///

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_Canvas = Template.FindName(nameof(PART_Canvas), this) as Canvas;
    }
}