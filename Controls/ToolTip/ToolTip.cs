using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ion.Controls;

[Extend<ToolTip>]
public static class XToolTip
{
    public static readonly ResourceKey HeaderPatternKey = new();

    #region Header

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached("Header", typeof(object), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static object GetHeader(FrameworkElement i) => i.GetValue(HeaderProperty);
    public static void SetHeader(FrameworkElement i, object input) => i.SetValue(HeaderProperty, input);

    #endregion

    #region HeaderIcon

    public static readonly DependencyProperty HeaderIconProperty = DependencyProperty.RegisterAttached("HeaderIcon", typeof(object), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static object GetHeaderIcon(FrameworkElement i) => i.GetValue(HeaderIconProperty);
    public static void SetHeaderIcon(FrameworkElement i, object input) => i.SetValue(HeaderIconProperty, input);

    #endregion

    #region HeaderIconTemplate

    public static readonly DependencyProperty HeaderIconTemplateProperty = DependencyProperty.RegisterAttached("HeaderIconTemplate", typeof(DataTemplate), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetHeaderIconTemplate(FrameworkElement i) => (DataTemplate)i.GetValue(HeaderIconTemplateProperty);
    public static void SetHeaderIconTemplate(FrameworkElement i, DataTemplate input) => i.SetValue(HeaderIconTemplateProperty, input);

    #endregion

    #region HeaderIconTemplateSelector

    public static readonly DependencyProperty HeaderIconTemplateSelectorProperty = DependencyProperty.RegisterAttached("HeaderIconTemplateSelector", typeof(DataTemplateSelector), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetHeaderIconTemplateSelector(FrameworkElement i) => (DataTemplateSelector)i.GetValue(HeaderIconTemplateSelectorProperty);
    public static void SetHeaderIconTemplateSelector(FrameworkElement i, DataTemplateSelector input) => i.SetValue(HeaderIconTemplateSelectorProperty, input);

    #endregion

    #region HeaderTemplate

    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetHeaderTemplate(FrameworkElement i) => (DataTemplate)i.GetValue(HeaderTemplateProperty);
    public static void SetHeaderTemplate(FrameworkElement i, DataTemplate input) => i.SetValue(HeaderTemplateProperty, input);

    #endregion

    #region HeaderTemplateSource

    public static readonly DependencyProperty HeaderTemplateSourceProperty = DependencyProperty.RegisterAttached("HeaderTemplateSource", typeof(Type), typeof(XToolTip), new FrameworkPropertyMetadata(null, OnHeaderTemplateSourceChanged));
    public static Type GetHeaderTemplateSource(FrameworkElement i) => (Type)i.GetValue(HeaderTemplateSourceProperty);
    public static void SetHeaderTemplateSource(FrameworkElement i, Type input) => i.SetValue(HeaderTemplateSourceProperty, input);

    private static void OnHeaderTemplateSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (GetHeaderTemplateSource(element) is Type type)
            {
                if (GetHeaderTemplateSourceKey(element) is string name)
                {
                    var result = Try.Get(() => type.GetField(name).GetValue(null) ?? type.GetProperty(name).GetValue(null));
                    result.IfNotNull(i => element.SetResourceReference(HeaderTemplateProperty, i));
                }
            }
        }
    }

    #endregion

    #region HeaderTemplateSourceKey

    public static readonly DependencyProperty HeaderTemplateSourceKeyProperty = DependencyProperty.RegisterAttached("HeaderTemplateSourceKey", typeof(string), typeof(XToolTip), new FrameworkPropertyMetadata(null, OnHeaderTemplateSourceChanged));
    public static string GetHeaderTemplateSourceKey(FrameworkElement i) => (string)i.GetValue(HeaderTemplateSourceKeyProperty);
    public static void SetHeaderTemplateSourceKey(FrameworkElement i, string input) => i.SetValue(HeaderTemplateSourceKeyProperty, input);

    #endregion

    #region HeaderTemplateSelector

    public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.RegisterAttached("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetHeaderTemplateSelector(FrameworkElement i) => (DataTemplateSelector)i.GetValue(HeaderTemplateProperty);
    public static void SetHeaderTemplateSelector(FrameworkElement i, DataTemplateSelector input) => i.SetValue(HeaderTemplateProperty, input);

    #endregion

    #region MaximumWidth

    public static readonly DependencyProperty MaximumWidthProperty = DependencyProperty.RegisterAttached("MaximumWidth", typeof(double), typeof(XToolTip), new FrameworkPropertyMetadata(720.0));
    public static double GetMaximumWidth(FrameworkElement i) => (double)i.GetValue(MaximumWidthProperty);
    public static void SetMaximumWidth(FrameworkElement i, double input) => i.SetValue(MaximumWidthProperty, input);

    #endregion

    #region MinimumWidth

    public static readonly DependencyProperty MinimumWidthProperty = DependencyProperty.RegisterAttached("MinimumWidth", typeof(double), typeof(XToolTip), new FrameworkPropertyMetadata(double.NaN));
    public static double GetMinimumWidth(FrameworkElement i) => (double)i.GetValue(MinimumWidthProperty);
    public static void SetMinimumWidth(FrameworkElement i, double input) => i.SetValue(MinimumWidthProperty, input);

    #endregion

    #region Template

    public static readonly DependencyProperty TemplateProperty = DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetTemplate(FrameworkElement i) => (DataTemplate)i.GetValue(TemplateProperty);
    public static void SetTemplate(FrameworkElement i, DataTemplate input) => i.SetValue(TemplateProperty, input);

    #endregion

    #region TemplateSelector

    public static readonly DependencyProperty TemplateSelectorProperty = DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(XToolTip), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetTemplateSelector(FrameworkElement i) => (DataTemplateSelector)i.GetValue(TemplateSelectorProperty);
    public static void SetTemplateSelector(FrameworkElement i, DataTemplateSelector input) => i.SetValue(TemplateSelectorProperty, input);

    #endregion

    #region TemplateSource

    public static readonly DependencyProperty TemplateSourceProperty = DependencyProperty.RegisterAttached("TemplateSource", typeof(Type), typeof(XToolTip), new FrameworkPropertyMetadata(null, OnTemplateSourceChanged));
    public static Type GetTemplateSource(FrameworkElement i) => (Type)i.GetValue(TemplateSourceProperty);
    public static void SetTemplateSource(FrameworkElement i, Type input) => i.SetValue(TemplateSourceProperty, input);

    private static void OnTemplateSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (GetTemplateSource(element) is Type type)
            {
                if (GetTemplateSourceKey(element) is string name)
                {
                    var result = Try.Get(() => type.GetField(name).GetValue(null) ?? type.GetProperty(name).GetValue(null));
                    result.IfNotNull(i => element.SetResourceReference(TemplateProperty, i));
                }
            }
        }
    }

    #endregion

    #region TemplateSourceKey

    public static readonly DependencyProperty TemplateSourceKeyProperty = DependencyProperty.RegisterAttached("TemplateSourceKey", typeof(string), typeof(XToolTip), new FrameworkPropertyMetadata(null, OnTemplateSourceChanged));
    public static string GetTemplateSourceKey(FrameworkElement i) => (string)i.GetValue(TemplateSourceKeyProperty);
    public static void SetTemplateSourceKey(FrameworkElement i, string input) => i.SetValue(TemplateSourceKeyProperty, input);

    #endregion

    #region Width

    public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width", typeof(double), typeof(XToolTip), new FrameworkPropertyMetadata(double.NaN));
    public static double GetWidth(FrameworkElement i) => (double)i.GetValue(WidthProperty);
    public static void SetWidth(FrameworkElement i, double input) => i.SetValue(WidthProperty, input);

    #endregion

    static XToolTip()
    {
        EventManager.RegisterClassHandler(typeof(ToolTip), ToolTip.OpenedEvent,
            new RoutedEventHandler(OnOpened), true);

        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
    }

    internal static void Initialize() { }

    /// <summary>https://stackoverflow.com/questions/9674508/wpf-tooltip-positioning/17050664#17050664</summary>
    private static void OnOpened(object sender, EventArgs e)
    {
        if (sender is ToolTip i)
        {
            if (i.PlacementTarget is null)
                return;

            var point = i.PlacementTarget.TranslatePoint(new Point(0, 0), i);
            if (point.Y > 0)
                i.Placement = PlacementMode.Top;

            i.Tag = new Thickness(point.X, 0, 0, 0);
        }
    }
}