using Ion.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ion.Controls;

[TemplatePart(Name = nameof(PART_Grid), Type = typeof(Grid))]
[TemplatePart(Name = nameof(PART_Rectangle), Type = typeof(Rectangle))]
public class DualColorBox : Control
{
    private Grid PART_Grid;
    private Rectangle PART_Rectangle;

    ///

    public event EventHandler<EventArgs<Color>> ForegroundColorChanged;

    public event EventHandler<EventArgs<Color>> BackgroundColorChanged;

    ///

    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(Color), typeof(DualColorBox), new FrameworkPropertyMetadata(System.Windows.Media.Colors.White, OnBackgroundColorChanged));
    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    private static void OnBackgroundColorChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DualColorBox>().OnBackgroundColorChanged(e.Convert<Color>());

    public static readonly DependencyProperty DefaultBackgroundProperty = DependencyProperty.Register(nameof(DefaultBackground), typeof(Color), typeof(DualColorBox), new FrameworkPropertyMetadata(System.Windows.Media.Colors.White));
    public Color DefaultBackground
    {
        get => (Color)GetValue(DefaultBackgroundProperty);
        set => SetValue(DefaultBackgroundProperty, value);
    }

    public static readonly DependencyProperty DefaultForegroundProperty = DependencyProperty.Register(nameof(DefaultForeground), typeof(Color), typeof(DualColorBox), new FrameworkPropertyMetadata(System.Windows.Media.Colors.Black));
    public Color DefaultForeground
    {
        get => (Color)GetValue(DefaultForegroundProperty);
        set => SetValue(DefaultForegroundProperty, value);
    }

    public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register(nameof(ForegroundColor), typeof(Color), typeof(DualColorBox), new FrameworkPropertyMetadata(System.Windows.Media.Colors.Black, OnForegroundColorChanged));
    public Color ForegroundColor
    {
        get => (Color)GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    private static void OnForegroundColorChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DualColorBox>().OnForegroundColorChanged(e.Convert<Color>());

    ///

    public DualColorBox() : base()
    {
        this.AddHandler(i =>
        {
            if (PART_Grid != null)
            {
                PART_Grid.PreviewMouseDown
                    += OnReset;
            }
            if (PART_Rectangle != null)
            {
                PART_Rectangle.PreviewMouseDown
                    += OnSwap;
            }
        }, i =>
        {
            if (PART_Grid != null)
            {
                PART_Grid.PreviewMouseDown
                    -= OnReset;
            }
            if (PART_Rectangle != null)
            {
                PART_Rectangle.PreviewMouseDown
                    -= OnSwap;
            }
        });
    }

    ///

    private void OnReset(object sender, MouseButtonEventArgs e)
    {
        BackgroundColor
            = DefaultBackground;
        ForegroundColor
            = DefaultForeground;
    }

    private void OnSwap(object sender, MouseButtonEventArgs e)
    {
        var a
            = ForegroundColor;
        var b
            = BackgroundColor;

        ForegroundColor = b;
        BackgroundColor = a;
    }

    ///

    protected virtual void OnBackgroundColorChanged(ValueChange<Color> input) => BackgroundColorChanged?.Invoke(this, new EventArgs<Color>(input.NewValue));

    protected virtual void OnForegroundColorChanged(ValueChange<Color> input) => ForegroundColorChanged?.Invoke(this, new EventArgs<Color>(input.NewValue));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_Grid
            = Template.FindName(nameof(PART_Grid), this) as Grid;
        PART_Rectangle
            = Template.FindName(nameof(PART_Rectangle), this) as Rectangle;
    }
}