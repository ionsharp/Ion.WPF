using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<TabControl>]
public static class XTabControl
{
    #region ContentVisibility

    public static readonly DependencyProperty ContentVisibilityProperty = DependencyProperty.RegisterAttached("ContentVisibility", typeof(Visibility), typeof(XTabControl), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetContentVisibility(TabControl i) => (Visibility)i.GetValue(ContentVisibilityProperty);
    public static void SetContentVisibility(TabControl i, Visibility value) => i.SetValue(ContentVisibilityProperty, value);

    #endregion

    #region HeaderPadding

    public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.RegisterAttached("HeaderPadding", typeof(Thickness), typeof(XTabControl), new FrameworkPropertyMetadata(default(Thickness)));
    public static Thickness GetHeaderPadding(TabControl i) => (Thickness)i.GetValue(HeaderPaddingProperty);
    public static void SetHeaderPadding(TabControl i, Thickness value) => i.SetValue(HeaderPaddingProperty, value);

    #endregion

    #region HeaderVisibility

    public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.RegisterAttached("HeaderVisibility", typeof(Visibility), typeof(XTabControl), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetHeaderVisibility(TabControl i) => (Visibility)i.GetValue(HeaderVisibilityProperty);
    public static void SetHeaderVisibility(TabControl i, Visibility value) => i.SetValue(HeaderVisibilityProperty, value);

    #endregion

    #region IsOverflowOpen

    public static readonly DependencyProperty IsOverflowOpenProperty = DependencyProperty.RegisterAttached("IsOverflowOpen", typeof(bool), typeof(XTabControl), new FrameworkPropertyMetadata(false));
    public static bool GetIsOverflowOpen(TabControl i) => (bool)i.GetValue(IsOverflowOpenProperty);
    public static void SetIsOverflowOpen(TabControl i, bool value) => i.SetValue(IsOverflowOpenProperty, value);

    #endregion

    #region OverflowCommand

    public static readonly DependencyProperty OverflowCommandProperty = DependencyProperty.RegisterAttached("OverflowCommand", typeof(ICommand), typeof(XTabControl), new FrameworkPropertyMetadata(null));
    public static ICommand GetOverflowCommand(TabControl i) => (ICommand)i.GetValue(OverflowCommandProperty);
    public static void SetOverflowCommand(TabControl i, ICommand value) => i.SetValue(OverflowCommandProperty, value);

    #endregion

    #region OverflowHeaderTemplate

    public static readonly DependencyProperty OverflowHeaderTemplateProperty = DependencyProperty.RegisterAttached("OverflowHeaderTemplate", typeof(DataTemplate), typeof(XTabControl), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetOverflowHeaderTemplate(TabControl i) => (DataTemplate)i.GetValue(OverflowHeaderTemplateProperty);
    public static void SetOverflowHeaderTemplate(TabControl i, DataTemplate value) => i.SetValue(OverflowHeaderTemplateProperty, value);

    #endregion

    #region OverflowIconTemplate

    public static readonly DependencyProperty OverflowIconTemplateProperty = DependencyProperty.RegisterAttached("OverflowIconTemplate", typeof(DataTemplate), typeof(XTabControl), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetOverflowIconTemplate(TabControl i) => (DataTemplate)i.GetValue(OverflowIconTemplateProperty);
    public static void SetOverflowIconTemplate(TabControl i, DataTemplate value) => i.SetValue(OverflowIconTemplateProperty, value);

    #endregion

    #region OverflowPanelOrientation

    public static readonly DependencyProperty OverflowPanelOrientationProperty = DependencyProperty.RegisterAttached("OverflowPanelOrientation", typeof(Orientation), typeof(XTabControl), new FrameworkPropertyMetadata(Orientation.Horizontal));
    public static Orientation GetOverflowPanelOrientation(TabControl i) => (Orientation)i.GetValue(OverflowPanelOrientationProperty);
    public static void SetOverflowPanelOrientation(TabControl i, Orientation value) => i.SetValue(OverflowPanelOrientationProperty, value);

    #endregion

    #region OverflowToolTipHeaderTemplate

    public static readonly DependencyProperty OverflowToolTipHeaderTemplateProperty = DependencyProperty.RegisterAttached("OverflowToolTipHeaderTemplate", typeof(DataTemplate), typeof(XTabControl), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetOverflowToolTipHeaderTemplate(TabControl i) => (DataTemplate)i.GetValue(OverflowToolTipHeaderTemplateProperty);
    public static void SetOverflowToolTipHeaderTemplate(TabControl i, DataTemplate value) => i.SetValue(OverflowToolTipHeaderTemplateProperty, value);

    #endregion

    #region OverflowToolTipHeaderIconTemplate

    public static readonly DependencyProperty OverflowToolTipHeaderIconTemplateProperty = DependencyProperty.RegisterAttached("OverflowToolTipHeaderIconTemplate", typeof(DataTemplate), typeof(XTabControl), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetOverflowToolTipHeaderIconTemplate(TabControl i) => (DataTemplate)i.GetValue(OverflowToolTipHeaderIconTemplateProperty);
    public static void SetOverflowToolTipHeaderIconTemplate(TabControl i, DataTemplate value) => i.SetValue(OverflowToolTipHeaderIconTemplateProperty, value);

    #endregion

    #region OverflowToolTipTemplate

    public static readonly DependencyProperty OverflowToolTipTemplateProperty = DependencyProperty.RegisterAttached("OverflowToolTipTemplate", typeof(DataTemplate), typeof(XTabControl), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetOverflowToolTipTemplate(TabControl i) => (DataTemplate)i.GetValue(OverflowToolTipTemplateProperty);
    public static void SetOverflowToolTipTemplate(TabControl i, DataTemplate value) => i.SetValue(OverflowToolTipTemplateProperty, value);

    #endregion
}