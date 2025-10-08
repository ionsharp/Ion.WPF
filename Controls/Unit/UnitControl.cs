using Ion.Data;
using Ion.Numeral;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ion.Controls;

public class UnitControl : Control
{
    public static readonly ResourceKey ComboBoxStyleKey = new();

    public static readonly ResourceKey DoubleUpDownStyleKey = new();

    ///

    public static readonly DependencyProperty ActualValueProperty = DependencyProperty.Register(nameof(ActualValue), typeof(double), typeof(UnitControl), new FrameworkPropertyMetadata(0.0));
    public double ActualValue
    {
        get => (double)GetValue(ActualValueProperty);
        set => SetValue(ActualValueProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(UnitControl), new FrameworkPropertyMetadata(false));
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register(nameof(Resolution), typeof(float), typeof(UnitControl), new FrameworkPropertyMetadata(72f, OnResolutionChanged));
    public float Resolution
    {
        get => (float)GetValue(ResolutionProperty);
        set => SetValue(ResolutionProperty, value);
    }

    private static void OnResolutionChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<UnitControl>().OnResolutionChanged(e.Convert<float>());

    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing), typeof(Thickness), typeof(UnitControl), new FrameworkPropertyMetadata(default(Thickness)));
    public Thickness Spacing
    {
        get => (Thickness)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(UnitControl), new FrameworkPropertyMetadata(null));
    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit), typeof(UnitType), typeof(UnitControl), new FrameworkPropertyMetadata(UnitType.Pixel, OnUnitChanged));
    public UnitType Unit
    {
        get => (UnitType)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    private static void OnUnitChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<UnitControl>().OnUnitChanged(e.Convert<UnitType>());

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(UnitControl), new FrameworkPropertyMetadata(0.0));
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private readonly BindingExpressionBase j;

    ///

    public UnitControl() : base()
    {
        j = this.Bind(ActualValueProperty, nameof(Value), this, BindingMode.TwoWay, new ValueConverter<double, double>
        (
            i => (double)new Unit(i.Value, UnitType.Pixel, Resolution).Convert(Unit),
            i => (double)new Unit(i.Value, Unit, Resolution).Convert(UnitType.Pixel)
        ));
    }

    ///

    protected virtual void OnResolutionChanged(ValueChange<float> i) => j?.UpdateTarget();

    protected virtual void OnUnitChanged(ValueChange<UnitType> i) => j?.UpdateTarget();
}