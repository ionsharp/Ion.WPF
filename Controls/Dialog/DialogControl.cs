using System.Windows;

namespace Ion.Controls;

public class DialogControl() : ContentControl<DialogModel>()
{
    public static readonly ResourceKey DropShadowEffectKey = new();

    public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(nameof(ButtonStyle), typeof(ButtonStyle), typeof(DialogControl), new FrameworkPropertyMetadata(ButtonStyle.Circle));
    public ButtonStyle ButtonStyle
    {
        get => (ButtonStyle)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }
}