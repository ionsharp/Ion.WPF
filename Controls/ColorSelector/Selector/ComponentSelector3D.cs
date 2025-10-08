using Ion.Colors;
using Ion.Numeral;
using System.Windows;

namespace Ion.Controls;

public class ComponentSelector3D() : ComponentSelector()
{
    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(nameof(Component), typeof(Component4), typeof(ComponentSelector3D), new FrameworkPropertyMetadata(Component4.X));
    public Component4 Component
    {
        get => (Component4)GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }

    protected override void Mark() { }

    protected override void OnMouseChanged(Vector2<Double1> input) { }
}