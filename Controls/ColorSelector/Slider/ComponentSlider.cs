using Ion.Colors;
using Ion.Numeral;
using System;
using System.Windows;

namespace Ion.Controls;

public class ComponentSlider : BaseComponentSlider
{
    private readonly Handle handle = false;

    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(nameof(Component), typeof(Component4), typeof(ComponentSlider), new FrameworkPropertyMetadata(Component4.X));
    public Component4 Component
    {
        get => (Component4)GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }

    public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(nameof(Shape), typeof(Polygon2D), typeof(ComponentSlider), new FrameworkPropertyMetadata(Polygon2D.Square));
    public Polygon2D Shape
    {
        get => (Polygon2D)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(Double1), typeof(ComponentSlider), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 X
    {
        get => (Double1)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(Double1), typeof(ComponentSlider), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 Y
    {
        get => (Double1)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public static readonly DependencyProperty ZProperty = DependencyProperty.Register(nameof(Z), typeof(Double1), typeof(ComponentSlider), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 Z
    {
        get => (Double1)GetValue(ZProperty);
        set => SetValue(ZProperty, value);
    }

    public static readonly DependencyProperty WProperty = DependencyProperty.Register(nameof(W), typeof(Double1), typeof(ComponentSlider), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 W
    {
        get => (Double1)GetValue(WProperty);
        set => SetValue(WProperty, value);
    }

    public ComponentSlider() : base() { }

    private static DependencyProperty GetProperty(Component4 input)
    {
        return input switch
        {
            Component4.X => XProperty,
            Component4.Y => YProperty,
            Component4.Z => ZProperty,
            Component4.W => WProperty,
            _ => throw new NotSupportedException(),
        };
    }

    protected override void Mark()
    {
        if (Shape == Polygon2D.Square)
        {
            var y = (Double1)GetValue(GetProperty(Component));

            ArrowPosition.X = 0;
            ArrowPosition.Y = ((1 - y) * ActualHeight) - 8;
        }
    }

    protected override void OnMouseChanged(Vector2<Double1> input)
    {
        base.OnMouseChanged(input);

        Double1 y = Shape == Polygon2D.Circle ? 1 - input.Distance() / new Vector2<Double1>(1, 1).Distance() : input.Y;
        handle.Do(() => SetCurrentValue(GetProperty(Component), y));

        if (Shape == Polygon2D.Circle)
        {
            ArrowPosition.X = (input.X * ActualWidth) - 6;
            ArrowPosition.Y = ((1 - input.Y) * ActualHeight) - 6;
        }
        else
        {
            ArrowPosition.X = 0;
            ArrowPosition.Y = ((1 - input.Y) * ActualHeight) - 8;
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        handle.DoInternal(() =>
        {
            if (e.Property == GetProperty(Component))
                Mark();
        });
    }
}