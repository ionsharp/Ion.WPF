using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<Expander>]
public static class XExpander
{
    #region HeaderPadding

    public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.RegisterAttached("HeaderPadding", typeof(Thickness), typeof(XExpander), new FrameworkPropertyMetadata(new Thickness(0)));
    public static Thickness GetHeaderPadding(Expander i) => (Thickness)i.GetValue(HeaderPaddingProperty);
    public static void SetHeaderPadding(Expander i, Thickness value) => i.SetValue(HeaderPaddingProperty, value);

    #endregion

    #region HeaderVisibility

    public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.RegisterAttached("HeaderVisibility", typeof(Visibility), typeof(XExpander), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetHeaderVisibility(Expander i) => (Visibility)i.GetValue(HeaderVisibilityProperty);
    public static void SetHeaderVisibility(Expander i, Visibility value) => i.SetValue(HeaderVisibilityProperty, value);

    #endregion
}