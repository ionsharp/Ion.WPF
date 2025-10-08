using Ion.Colors;
using Ion.Numeral;
using System.Windows;
using static System.Math;

namespace Ion.Controls;

public class ComponentSelector2D : ComponentSelector
{
    private readonly Handle handle = false;

    public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(nameof(Shape), typeof(Polygon2D), typeof(ComponentSelector2D), new FrameworkPropertyMetadata(Polygon2D.Square));
    public Polygon2D Shape
    {
        get => (Polygon2D)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public static readonly DependencyProperty XComponentProperty = DependencyProperty.Register(nameof(XComponent), typeof(Component4), typeof(ComponentSelector2D), new FrameworkPropertyMetadata(Component4.X));
    public Component4 XComponent
    {
        get => (Component4)GetValue(XComponentProperty);
        set => SetValue(XComponentProperty, value);
    }

    public static readonly DependencyProperty YComponentProperty = DependencyProperty.Register(nameof(YComponent), typeof(Component4), typeof(ComponentSelector2D), new FrameworkPropertyMetadata(Component4.Y));
    public Component4 YComponent
    {
        get => (Component4)GetValue(YComponentProperty);
        set => SetValue(YComponentProperty, value);
    }

    public ComponentSelector2D() : base() { }

    private void Mark(double x, double y)
    {
        EllipsePosition.X = ((x * ActualWidth) - (EllipseDiameter / 2.0)).Round();
        EllipsePosition.Y = ((ActualHeight - (y * ActualHeight)) - (EllipseDiameter / 2.0)).Round();
    }

    protected override void Mark()
    {
        var x = GetValue(XComponent);
        var y = GetValue(YComponent);
        Mark(x, y);
    }

    protected override void OnMouseChanged(Vector2<Double1> input)
    {
        base.OnMouseChanged(input);

        double x = input.X, y = input.Y;
        if (Shape == Polygon2D.Circle)
        {
            double xN = 1 - input.Distance() / Vector2.One.Distance();
            double yN = Atan2(y - 0.5, x - 0.5) + new Angle(270).Convert(AngleType.Radian); //Replace with variable later...
            yN += PI;
            yN = yN > PI * 2 ? yN - PI * 2 : yN;
            yN /= PI * 2;

            x = xN;
            y = yN;
        }

        handle.DoInternal(() =>
        {
            SetValue(XComponent, x);
            SetValue(YComponent, y);
        });

        Mark(input.X, input.Y);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        handle.DoInternal(() =>
        {
            if (e.Property == XProperty && (XComponent == Component4.X || YComponent == Component4.X))
                Mark();

            if (e.Property == YProperty && (XComponent == Component4.Y || YComponent == Component4.Y))
                Mark();

            if (e.Property == ZProperty && (XComponent == Component4.Z || YComponent == Component4.Z))
                Mark();

            if (e.Property == WProperty && (XComponent == Component4.W || YComponent == Component4.W))
                Mark();
        });
    }
}