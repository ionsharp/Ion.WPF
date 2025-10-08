using Ion.Numeral;
using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

public class AlphaSlider : BaseComponentSlider
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(AlphaSlider), new FrameworkPropertyMetadata(default(Color)));
    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(byte), typeof(AlphaSlider), new FrameworkPropertyMetadata(default(byte), OnValueChanged));
    public byte Value
    {
        get => (byte)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<AlphaSlider>().OnValueChanged(e);

    public AlphaSlider() : base() { }

    protected override void Mark() => ArrowPosition.Y = ((1 - Value.Normalize()) * ActualHeight) - 8;

    protected override void OnMouseChanged(Vector2<Double1> input)
    {
        base.OnMouseChanged(input);
        SetCurrentValue(ValueProperty, input.Y.To<double>().Denormalize<byte>());
    }

    protected virtual void OnValueChanged(Value<byte> input) => Mark();
}