using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<Separator>]
public static class XSeparator
{
    #region Header

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached("Header", typeof(object), typeof(XSeparator), new FrameworkPropertyMetadata(null));
    public static object GetHeader(Separator i) => i.GetValue(HeaderProperty);
    public static void SetHeader(Separator i, object value) => i.SetValue(HeaderProperty, value);

    #endregion

    #region HeaderTemplate

    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(XSeparator), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetHeaderTemplate(Separator i) => (DataTemplate)i.GetValue(HeaderTemplateProperty);
    public static void SetHeaderTemplate(Separator i, DataTemplate value) => i.SetValue(HeaderTemplateProperty, value);

    #endregion

    #region Icon

    public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(object), typeof(XSeparator), new FrameworkPropertyMetadata(null));
    public static object GetIcon(Separator i) => i.GetValue(IconProperty);
    public static void SetIcon(Separator i, object value) => i.SetValue(IconProperty, value);

    #endregion

    #region IconTemplate

    public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.RegisterAttached("IconTemplate", typeof(DataTemplate), typeof(XSeparator), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetIconTemplate(Separator i) => (DataTemplate)i.GetValue(IconTemplateProperty);
    public static void SetIconTemplate(Separator i, DataTemplate value) => i.SetValue(IconTemplateProperty, value);

    #endregion
}