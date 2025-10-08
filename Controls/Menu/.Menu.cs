using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<Menu>]
public static class XMenu
{
    public static readonly ResourceKey GroupStyle = new();

    #region TopLevelIconVisibility

    public static readonly DependencyProperty TopLevelIconVisibilityProperty = DependencyProperty.RegisterAttached("TopLevelIconVisibility", typeof(Visibility), typeof(XMenu), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetTopLevelIconVisibility(Menu i) => (Visibility)i.GetValue(TopLevelIconVisibilityProperty);
    public static void SetTopLevelIconVisibility(Menu i, Visibility input) => i.SetValue(TopLevelIconVisibilityProperty, input);

    #endregion
}