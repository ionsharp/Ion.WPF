using Ion.Colors;
using Ion.Numeral;
using Ion.Numeral.Models;
using Ion.Reflect;
using System;
using System.Windows;

namespace Ion.Controls;

public abstract class ComponentSelector : ColorSelector
{
    public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register(nameof(EllipseDiameter), typeof(double), typeof(ComponentSelector), new FrameworkPropertyMetadata(12.0));
    public double EllipseDiameter
    {
        get => (double)GetValue(EllipseDiameterProperty);
        set => SetValue(EllipseDiameterProperty, value);
    }

    public static readonly DependencyProperty EllipsePositionProperty = DependencyProperty.Register(nameof(EllipsePosition), typeof(Vector2M<double>), typeof(ComponentSelector), new FrameworkPropertyMetadata(null));
    public Vector2M<double> EllipsePosition
    {
        get => (Vector2M<double>)GetValue(EllipsePositionProperty);
        private set => SetValue(EllipsePositionProperty, value);
    }

    public static readonly DependencyPropertyKey Is4Key = DependencyProperty.RegisterReadOnly(nameof(Is4), typeof(bool), typeof(ComponentSelector), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty Is4Property = Is4Key.DependencyProperty;
    public bool Is4
    {
        get => (bool)GetValue(Is4Property);
        protected set => SetValue(Is4Key, value);
    }

    public static readonly DependencyProperty RotateXProperty = DependencyProperty.Register(nameof(RotateX), typeof(double), typeof(ComponentSelector), new FrameworkPropertyMetadata(45.0));
    public double RotateX
    {
        get => (double)GetValue(RotateXProperty);
        set => SetValue(RotateXProperty, value);
    }

    public static readonly DependencyProperty RotateYProperty = DependencyProperty.Register(nameof(RotateY), typeof(double), typeof(ComponentSelector), new FrameworkPropertyMetadata(45.0));
    public double RotateY
    {
        get => (double)GetValue(RotateYProperty);
        set => SetValue(RotateYProperty, value);
    }

    public static readonly DependencyProperty RotateZProperty = DependencyProperty.Register(nameof(RotateZ), typeof(double), typeof(ComponentSelector), new FrameworkPropertyMetadata(0.0));
    public double RotateZ
    {
        get => (double)GetValue(RotateZProperty);
        set => SetValue(RotateZProperty, value);
    }

    public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(Double1), typeof(ComponentSelector), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 X
    {
        get => (Double1)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(Double1), typeof(ComponentSelector), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 Y
    {
        get => (Double1)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public static readonly DependencyProperty ZProperty = DependencyProperty.Register(nameof(Z), typeof(Double1), typeof(ComponentSelector), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 Z
    {
        get => (Double1)GetValue(ZProperty);
        set => SetValue(ZProperty, value);
    }

    public static readonly DependencyProperty WProperty = DependencyProperty.Register(nameof(W), typeof(Double1), typeof(ComponentSelector), new FrameworkPropertyMetadata(Double1.Zero));
    public Double1 W
    {
        get => (Double1)GetValue(WProperty);
        set => SetValue(WProperty, value);
    }

    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(ComponentSelector), new FrameworkPropertyMetadata(2.0));
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    ///

    protected ComponentSelector() : base()
    {
        SetCurrentValue(EllipsePositionProperty, new Vector2M<double>(0, 0));
    }

    ///

    protected Double1 GetValue(Component4 component)
    {
        return component switch
        {
            Component4.X => X,
            Component4.Y => Y,
            Component4.Z => Z,
            Component4.W => W,
            _ => throw new NotSupportedException(),
        };
    }

    protected void SetValue(Component4 component, Double1 value)
    {
        switch (component)
        {
            case Component4.X:
                SetCurrentValue(XProperty, value);
                break;
            case Component4.Y:
                SetCurrentValue(YProperty, value);
                break;
            case Component4.Z:
                SetCurrentValue(ZProperty, value);
                break;
            case Component4.W:
                SetCurrentValue(WProperty, value);
                break;
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ModelProperty)
            Is4 = Model.Implements<IColor4>();
    }
}