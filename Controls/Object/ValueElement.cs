using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class ValueElement() : ContentPresenter()
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orient), typeof(ValueElement), new FrameworkPropertyMetadata(Orient.Vertical));
    public Orient Orientation
    {
        get => (Orient)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}