using Ion.Data;
using Ion.Numeral;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ion.Controls;

public class AngleControl : Control
{
    public static readonly ReferenceKey<Ellipse> EllipseKey = new();

    public static readonly ReferenceKey<Line> LineKey = new();

    #region Properties

    public static readonly DependencyProperty DegreesProperty = DependencyProperty.Register(nameof(Degrees), typeof(double), typeof(AngleControl), new FrameworkPropertyMetadata(0d, OnDegreesChanged));
    public double Degrees
    {
        get => (double)GetValue(DegreesProperty);
        set => SetValue(DegreesProperty, value);
    }
    private static void OnDegreesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => d.As<AngleControl>().OnDegreesChanged(e.Convert<double>());

    public static readonly DependencyProperty OriginFillProperty = DependencyProperty.Register(nameof(OriginFill), typeof(Brush), typeof(AngleControl), new FrameworkPropertyMetadata(Brushes.Black));
    public Brush OriginFill
    {
        get => (Brush)GetValue(OriginFillProperty);
        set => SetValue(OriginFillProperty, value);
    }

    public static readonly DependencyProperty OriginStrokeProperty = DependencyProperty.Register(nameof(OriginStroke), typeof(Brush), typeof(AngleControl), new FrameworkPropertyMetadata(Brushes.Black));
    public Brush OriginStroke
    {
        get => (Brush)GetValue(OriginStrokeProperty);
        set => SetValue(OriginStrokeProperty, value);
    }

    public static readonly DependencyProperty OriginStrokeThicknessProperty = DependencyProperty.Register(nameof(OriginStrokeThickness), typeof(double), typeof(AngleControl), new FrameworkPropertyMetadata(8d));
    public double OriginStrokeThickness
    {
        get => (double)GetValue(OriginStrokeThicknessProperty);
        set => SetValue(OriginStrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty OriginVisibilityProperty = DependencyProperty.Register(nameof(OriginVisibility), typeof(Visibility), typeof(AngleControl), new FrameworkPropertyMetadata(Visibility.Visible));
    public Visibility OriginVisibility
    {
        get => (Visibility)GetValue(OriginVisibilityProperty);
        set => SetValue(OriginVisibilityProperty, value);
    }

    public static readonly DependencyProperty NeedleStrokeProperty = DependencyProperty.Register(nameof(NeedleStroke), typeof(Brush), typeof(AngleControl), new FrameworkPropertyMetadata(Brushes.Black));
    public Brush NeedleStroke
    {
        get => (Brush)GetValue(NeedleStrokeProperty);
        set => SetValue(NeedleStrokeProperty, value);
    }

    public static readonly DependencyProperty NeedleStrokeThicknessProperty = DependencyProperty.Register(nameof(NeedleStrokeThickness), typeof(double), typeof(AngleControl), new FrameworkPropertyMetadata(2d));
    public double NeedleStrokeThickness
    {
        get => (double)GetValue(NeedleStrokeThicknessProperty);
        set => SetValue(NeedleStrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty RadiansProperty = DependencyProperty.Register(nameof(Radians), typeof(double), typeof(AngleControl), new FrameworkPropertyMetadata(0d, OnRadiansChanged));
    public double Radians
    {
        get => (double)GetValue(RadiansProperty);
        set => SetValue(RadiansProperty, value);
    }
    private static void OnRadiansChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => d.As<AngleControl>().OnRadiansChanged(e.Convert<double>());

    #endregion

    #region AngleControl

    private readonly BindingExpressionBase j;

    public AngleControl() : base()
    {
        this.Bind(HeightProperty, nameof(Width), this, BindingMode.TwoWay);
        j = this.Bind(RadiansProperty, nameof(Degrees), this, BindingMode.TwoWay, new ValueConverter<double, double>
        (
            i => (double)new Angle(i.Value, AngleType.Degree).Convert(AngleType.Radian),
            i => (double)new Angle(i.Value, AngleType.Radian).Convert(AngleType.Degree)
        ));

        this.AddHandler(i =>
        {
            this.GetChild(EllipseKey).IfNotNull(j =>
            {
                j.MouseDown
                    += OnMouseDown;
                j.MouseMove
                    += OnMouseMove;
                j.MouseUp
                    += OnMouseUp;
            });
            CenterLine();
        }, i =>
        {
            this.GetChild(EllipseKey).IfNotNull(j =>
            {
                j.MouseDown
                    -= OnMouseDown;
                j.MouseMove
                    -= OnMouseMove;
                j.MouseUp
                    -= OnMouseUp;
            });
        });
    }

    #endregion

    #region Methods

    private void CenterLine() => this.GetChild(LineKey).IfNotNull(i =>
    {
        i.X1 = i.X2 = ActualWidth / 2d;
        i.Y2 = ActualHeight / 2d;
    });

    private void UpdateLine() => this.GetChild(LineKey).IfNotNull(i => i.RenderTransform = new RotateTransform(Degrees + 90d));

    ///

    private double RadiansFromPoint(Point point)
    {
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        point.X = Math.Clamp(point.X, 0, ActualWidth);
        point.Y = Math.Clamp(point.Y, 0, ActualHeight);
        return Math.Atan2(point.Y - center.Y, point.X - center.X);
    }

    ///

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (sender is Ellipse i)
            {
                i.CaptureMouse();
                SetCurrentValue(RadiansProperty, RadiansFromPoint(e.GetPosition(i)));
            }
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (sender is Ellipse i)
                SetCurrentValue(RadiansProperty, RadiansFromPoint(e.GetPosition(i)));
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released)
        {
            if (sender is Ellipse i)
            {
                if (i.IsMouseCaptured)
                    i.ReleaseMouseCapture();
            }
        }
    }

    ///

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        CenterLine();
        j?.UpdateTarget();
    }

    ///

    protected virtual void OnDegreesChanged(ValueChange<double> input) => UpdateLine();

    protected virtual void OnRadiansChanged(ValueChange<double> input) => UpdateLine();

    #endregion
}