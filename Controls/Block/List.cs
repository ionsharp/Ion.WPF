using System.Windows;
using System.Windows.Documents;

namespace Ion.Controls;

[Extend<List>]
public static class XListBlock
{
    #region FontScale

    public static readonly DependencyProperty FontScaleProperty = DependencyProperty.RegisterAttached("FontScale", typeof(double), typeof(XListBlock), new FrameworkPropertyMetadata(1.0, OnFontScaleChanged));
    public static double GetFontScale(List i) => (double)i.GetValue(FontScaleProperty);
    public static void SetFontScale(List i, double input) => i.SetValue(FontScaleProperty, input);

    private static void OnFontScaleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var list = sender as List;
        list.FontSize = GetFontScaleOrigin(list) * (double)e.NewValue;
    }

    #endregion

    #region FontScaleOrigin

    public static readonly DependencyProperty FontScaleOriginProperty = DependencyProperty.RegisterAttached("FontScaleOrigin", typeof(double), typeof(XListBlock), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, OnFontScaleOriginChanged));
    public static double GetFontScaleOrigin(List i) => (double)i.GetValue(FontScaleOriginProperty);
    public static void SetFontScaleOrigin(List i, double input) => i.SetValue(FontScaleOriginProperty, input);

    private static void OnFontScaleOriginChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var list = sender as List;
        list.FontSize = (double)e.NewValue * GetFontScale(list);
    }

    #endregion
}