using Ion.Colors;
using Ion.Reflect;
using System.Windows;

namespace Ion.Controls;

public class GradientBox : PickerBox<Gradient>
{
    protected override Gradient DefaultValue => Gradient.Default;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Gradient), typeof(GradientBox), new FrameworkPropertyMetadata(null, OnValueChanged));
    public Gradient Value
    {
        get => (Gradient)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<GradientBox>().OnValueChanged(e);

    public GradientBox() : base() { }

    protected override Gradient GetValue() => Instance.CloneDeep(Value) as Gradient ?? DefaultValue;

    protected override void SetValue(Gradient i) => Value.CopyFrom(i);

    public override void ShowDialog()
        => Dialog.ShowObject(Title, GetValue(), Resource.GetImageUri(Images.Gradient), Buttons.Done);
}