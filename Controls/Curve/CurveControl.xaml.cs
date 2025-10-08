using Ion.Numeral;
using Microsoft.CSharp;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ion.Controls;

public partial class CurveControl : UserControl
{
    public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(nameof(Code), typeof(string), typeof(CurveControl), new FrameworkPropertyMetadata("", OnCodeChanged));
    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }
    static void OnCodeChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.As<CurveControl>().Update();

    public static readonly DependencyProperty CurveProperty = DependencyProperty.Register(nameof(Curve), typeof(Curve), typeof(CurveControl), new FrameworkPropertyMetadata(null, OnCurveChanged));
    public Curve Curve
    {
        get => (Curve)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }
    static void OnCurveChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.As<CurveControl>().Update();

    public static readonly DependencyProperty QualityProperty = DependencyProperty.Register(nameof(Quality), typeof(double), typeof(CurveControl), new FrameworkPropertyMetadata(12.5, OnQualityChanged));
    public double Quality
    {
        get => (double)GetValue(QualityProperty);
        set => SetValue(QualityProperty, value);
    }
    static void OnQualityChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.As<CurveControl>().Update();

    static readonly DependencyPropertyKey PreviewXKey = DependencyProperty.RegisterReadOnly(nameof(PreviewX), typeof(PointCollection), typeof(CurveControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty PreviewXProperty = PreviewXKey.DependencyProperty;
    public PointCollection PreviewX
    {
        get => (PointCollection)GetValue(PreviewXProperty);
        private set => SetValue(PreviewXKey, value);
    }

    static readonly DependencyPropertyKey PreviewYKey = DependencyProperty.RegisterReadOnly(nameof(PreviewY), typeof(PointCollection), typeof(CurveControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty PreviewYProperty = PreviewYKey.DependencyProperty;
    public PointCollection PreviewY
    {
        get => (PointCollection)GetValue(PreviewYProperty);
        private set => SetValue(PreviewYKey, value);
    }

    ///

    public CurveControl() : base() => InitializeComponent();

    ///

    PointCollection GetPreview()
    {
        var result = new PointCollection();
        CSharpCodeProvider provider = null;

        Func<double, double> function = new((Curve as ICurve).Do);
        if (Curve is not CurveInverse)
        {
            if (Code?.Length > 0)
            {
                provider = CurveCompile.Compile(Code, out MethodInfo method);
                function = new(i => (double)method.Invoke(null, [0, 1, i]));
            }
            else return result;
        }

        //Example: Quality = 50.0 (maximum), increment = 0.02
        var increment = 1.0 / Quality;
        for (var x = 0d; x < 1; x += increment)
            result.Add(new(x, 1 - function(x)));

        result.Add(new(1, 1 - function(1)));

        function = null;
        provider?.Dispose();

        return result;
    }

    void Update()
    {
        if (Curve != null)
        {
            var x = GetPreview();
            x.Add(new(1, 0));
            x.Add(new(0, 0));
            x.Add(x[0]);
            PreviewX = x;

            var y = GetPreview();
            y.Add(new(1, 1));
            y.Add(new(0, 1));
            y.Add(y[0]);
            PreviewY = y;
        }
    }
}