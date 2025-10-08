using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

[Extend<Button>]
public static class XButton
{
    public static readonly ResourceKey DefaultStyle = new();

    ///

    public static readonly ResourceKey BlackButton = new();

    public static readonly ResourceKey BlueButton = new();

    public static readonly ResourceKey GreenButton = new();

    public static readonly ResourceKey OrangeButton = new();

    public static readonly ResourceKey RedButton = new();

    public static readonly ResourceKey TransparentButton = new();

    #region Image

    public static readonly DependencyProperty ImageProperty = DependencyProperty.RegisterAttached("Image", typeof(object), typeof(XButton), new FrameworkPropertyMetadata(null));
    public static object GetImage(Button i) => i.GetValue(ImageProperty);
    public static void SetImage(Button i, ImageSource value) => i.SetValue(ImageProperty, value);

    #endregion

    #region ImageTemplate

    public static readonly DependencyProperty ImageTemplateProperty = DependencyProperty.RegisterAttached("ImageTemplate", typeof(DataTemplate), typeof(XButton), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetImageTemplate(Button i) => (DataTemplate)i.GetValue(ImageTemplateProperty);
    public static void SetImageTemplate(Button i, DataTemplate value) => i.SetValue(ImageTemplateProperty, value);

    #endregion

    #region IsCheckable

    public static readonly DependencyProperty IsCheckableProperty = DependencyProperty.RegisterAttached("IsCheckable", typeof(bool), typeof(XButton), new FrameworkPropertyMetadata(false));
    public static bool GetIsCheckable(Button i) => (bool)i.GetValue(IsCheckableProperty);
    public static void SetIsCheckable(Button i, bool value) => i.SetValue(IsCheckableProperty, value);

    #endregion

    #region IsChecked

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached("IsChecked", typeof(bool), typeof(XButton), new FrameworkPropertyMetadata(false, null, OnIsCheckedCoerced));
    public static bool GetIsChecked(Button i) => (bool)i.GetValue(IsCheckedProperty);
    public static void SetIsChecked(Button i, bool value) => i.SetValue(IsCheckedProperty, value);
    private static object OnIsCheckedCoerced(DependencyObject i, object value) => !GetIsCheckable(i as Button) ? false : value ?? false;

    #endregion

    #region Menu

    public static readonly DependencyProperty MenuProperty = DependencyProperty.RegisterAttached("Menu", typeof(object), typeof(XButton), new FrameworkPropertyMetadata(null));
    public static object GetMenu(Button i) => i.GetValue(MenuProperty);
    public static void SetMenu(Button i, object value) => i.SetValue(MenuProperty, value);

    #endregion

    #region MenuTemplate

    public static readonly DependencyProperty MenuTemplateProperty = DependencyProperty.RegisterAttached("MenuTemplate", typeof(DataTemplate), typeof(XButton), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetMenuTemplate(Button i) => (DataTemplate)i.GetValue(MenuTemplateProperty);
    public static void SetMenuTemplate(Button i, DataTemplate value) => i.SetValue(MenuTemplateProperty, value);

    #endregion

    #region MenuMaxHeight

    public static readonly DependencyProperty MenuMaxHeightProperty = DependencyProperty.RegisterAttached("MenuMaxHeight", typeof(double), typeof(XButton), new FrameworkPropertyMetadata(720.0));
    public static double GetMenuMaxHeight(Button i) => (double)i.GetValue(MenuMaxHeightProperty);
    public static void SetMenuMaxHeight(Button i, double value) => i.SetValue(MenuMaxHeightProperty, value);

    #endregion

    #region MenuMaxWidth

    public static readonly DependencyProperty MenuMaxWidthProperty = DependencyProperty.RegisterAttached("MenuMaxWidth", typeof(double), typeof(XButton), new FrameworkPropertyMetadata(540.0));
    public static double GetMenuMaxWidth(Button i) => (double)i.GetValue(MenuMaxWidthProperty);
    public static void SetMenuMaxWidth(Button i, double value) => i.SetValue(MenuMaxWidthProperty, value);

    #endregion

    #region MenuMinWidth

    public static readonly DependencyProperty MenuMinWidthProperty = DependencyProperty.RegisterAttached("MenuMinWidth", typeof(double), typeof(XButton), new FrameworkPropertyMetadata(360.0));
    public static double GetMenuMinWidth(Button i) => (double)i.GetValue(MenuMinWidthProperty);
    public static void SetMenuMinWidth(Button i, double value) => i.SetValue(MenuMinWidthProperty, value);

    #endregion

    #region Result

    public static readonly DependencyProperty ResultProperty = DependencyProperty.RegisterAttached("Result", typeof(int), typeof(XButton), new FrameworkPropertyMetadata(-1));
    public static int GetResult(Button i) => (int)i.GetValue(ResultProperty);
    public static void SetResult(Button i, int value) => i.SetValue(ResultProperty, value);

    #endregion

    static XButton()
    {
        EventManager.RegisterClassHandler(typeof(Button), UIElement.PreviewMouseLeftButtonUpEvent,
            new MouseButtonEventHandler(OnPreviewMouseLeftButtonUp), true);
    }

    private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button button)
        {
            if (GetIsCheckable(button))
                SetIsChecked(button, !GetIsChecked(button));
        }
    }
}