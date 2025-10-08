using Ion.Numeral;
using System;
using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

public partial class SplashWindow : Window
{
    public const double DefaultImageHeight = 300;

    public const double DefaultImageWidth = 512;

    /// <see cref="Region.Field"/>

    public static readonly ResourceKey DefaultStyle = new();

    public static readonly ResourceKey DropShadowEffectKey = new();

    public event EventHandler<EventArgs> Shown;

    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(nameof(Accent), typeof(Color), typeof(SplashWindow), new FrameworkPropertyMetadata(default(Color)));
    public Color Accent
    {
        get => (Color)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(nameof(Delay), typeof(int), typeof(SplashWindow), new FrameworkPropertyMetadata(0));
    public int Delay
    {
        get => (int)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(SplashWindow), new FrameworkPropertyMetadata(null));
    public ImageSource Image
    {
        get => (ImageSource)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(nameof(ImageHeight), typeof(double), typeof(SplashWindow), new FrameworkPropertyMetadata(DefaultImageHeight));
    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(nameof(ImageWidth), typeof(double), typeof(SplashWindow), new FrameworkPropertyMetadata(DefaultImageWidth));
    public double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public static readonly DependencyProperty ImageShapeProperty = DependencyProperty.Register(nameof(ImageShape), typeof(Polygon2D), typeof(SplashWindow), new FrameworkPropertyMetadata(Polygon2D.Square));
    public Polygon2D ImageShape
    {
        get => (Polygon2D)GetValue(ImageShapeProperty);
        set => SetValue(ImageShapeProperty, value);
    }

    public static readonly DependencyProperty ImageTemplateProperty = DependencyProperty.Register(nameof(ImageTemplate), typeof(DataTemplate), typeof(SplashWindow), new FrameworkPropertyMetadata(null));
    public DataTemplate ImageTemplate
    {
        get => (DataTemplate)GetValue(ImageTemplateProperty);
        set => SetValue(ImageTemplateProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(SplashWindow), new FrameworkPropertyMetadata(null));
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(SplashWindow), new FrameworkPropertyMetadata(0d));
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    static SplashWindow()
    {
        OpacityProperty
            .OverrideMetadata(typeof(SplashWindow), new FrameworkPropertyMetadata(0d, OnOpacityChanged));
        WindowStateProperty
            .OverrideMetadata(typeof(SplashWindow), new FrameworkPropertyMetadata(WindowState.Normal, null, OnWindowStateCoerced));
    }

    public SplashWindow() : base() => InitializeComponent();

    private static void OnOpacityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if ((double)e.NewValue == 1)
        {
            if (sender is SplashWindow window)
                window.Shown?.Invoke(window, EventArgs.Empty);
        }
    }

    private static object OnWindowStateCoerced(DependencyObject sender, object input) => WindowState.Normal;
}