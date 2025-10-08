using System.Windows;

namespace Ion.Controls;

public class DisplayButton() : Display<Window>()
{

    public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(nameof(ButtonStyle), typeof(Style), typeof(DisplayButton), new FrameworkPropertyMetadata(null));
    public Style ButtonStyle
    {
        get => (Style)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }
}