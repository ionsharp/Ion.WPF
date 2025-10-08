using System;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<ContextMenu>]
public static class XContextMenu
{
    public static readonly ResourceKey HeaderPatternKey = new();

    #region Above

    public static readonly DependencyProperty AboveProperty = DependencyProperty.RegisterAttached("Above", typeof(object), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static object GetAbove(ContextMenu i) => i.GetValue(AboveProperty);
    public static void SetAbove(ContextMenu i, object input) => i.SetValue(AboveProperty, input);

    #endregion

    #region AboveTemplate

    public static readonly DependencyProperty AboveTemplateProperty = DependencyProperty.RegisterAttached("AboveTemplate", typeof(DataTemplate), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetAboveTemplate(ContextMenu i) => (DataTemplate)i.GetValue(AboveTemplateProperty);
    public static void SetAboveTemplate(ContextMenu i, DataTemplate input) => i.SetValue(AboveTemplateProperty, input);

    #endregion

    #region Below

    public static readonly DependencyProperty BelowProperty = DependencyProperty.RegisterAttached("Below", typeof(object), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static object GetBelow(ContextMenu i) => i.GetValue(BelowProperty);
    public static void SetBelow(ContextMenu i, object input) => i.SetValue(BelowProperty, input);

    #endregion

    #region BelowTemplate

    public static readonly DependencyProperty BelowTemplateProperty = DependencyProperty.RegisterAttached("BelowTemplate", typeof(DataTemplate), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetBelowTemplate(ContextMenu i) => (DataTemplate)i.GetValue(BelowTemplateProperty);
    public static void SetBelowTemplate(ContextMenu i, DataTemplate input) => i.SetValue(BelowTemplateProperty, input);

    #endregion

    #region DataType

    public static readonly DependencyProperty DataTypeProperty = DependencyProperty.RegisterAttached("DataType", typeof(Type), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static Type GetDataType(ContextMenu i) => (Type)i.GetValue(DataTypeProperty);
    public static void SetDataType(ContextMenu i, Type input) => i.SetValue(DataTypeProperty, input);

    #endregion

    #region DataKey

    public static readonly DependencyProperty DataKeyProperty = DependencyProperty.RegisterAttached("DataKey", typeof(object), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static object GetDataKey(ContextMenu i) => i.GetValue(DataKeyProperty);
    public static void SetDataKey(ContextMenu i, object input) => i.SetValue(DataKeyProperty, input);

    #endregion

    #region Header

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached("Header", typeof(object), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static object GetHeader(ContextMenu i) => i.GetValue(HeaderProperty);
    public static void SetHeader(ContextMenu i, object input) => i.SetValue(HeaderProperty, input);

    #endregion

    #region HeaderIcon

    public static readonly DependencyProperty HeaderIconProperty = DependencyProperty.RegisterAttached("HeaderIcon", typeof(object), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static object GetHeaderIcon(ContextMenu i) => i.GetValue(HeaderIconProperty);
    public static void SetHeaderIcon(ContextMenu i, object input) => i.SetValue(HeaderIconProperty, input);

    #endregion

    #region HeaderIconTemplate

    public static readonly DependencyProperty HeaderIconTemplateProperty = DependencyProperty.RegisterAttached("HeaderIconTemplate", typeof(DataTemplate), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetHeaderIconTemplate(ContextMenu i) => (DataTemplate)i.GetValue(HeaderIconTemplateProperty);
    public static void SetHeaderIconTemplate(ContextMenu i, DataTemplate input) => i.SetValue(HeaderIconTemplateProperty, input);

    #endregion

    #region HeaderTemplate

    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(XContextMenu), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetHeaderTemplate(ContextMenu i) => (DataTemplate)i.GetValue(HeaderTemplateProperty);
    public static void SetHeaderTemplate(ContextMenu i, DataTemplate input) => i.SetValue(HeaderTemplateProperty, input);

    #endregion
}