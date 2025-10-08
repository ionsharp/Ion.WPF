using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<Border>]
public static class XBorder
{
    #region Clip

    /// <summary>
    /// To do: Migrate <see cref="ClipBorder"/>.
    /// </summary>
    public static readonly DependencyProperty ClipProperty = DependencyProperty.RegisterAttached("Clip", typeof(bool), typeof(XBorder), new FrameworkPropertyMetadata(false, OnClipChanged));
    public static bool GetClip(Border i) => (bool)i.GetValue(ClipProperty);
    public static void SetClip(Border i, bool input) => i.SetValue(ClipProperty, input);

    private static void OnClipChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Border)
        {
        }
    }

    #endregion
}