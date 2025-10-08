using Ion.Numeral;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<Popup>]
public static class XPopup
{
    public static readonly ResourceKey DropShadowEffectKey = new();

    #region Fields

    public static readonly Range<double> Height = new(MinimumHeight, MaximumHeight);

    public const double MaximumHeight = 720;

    public const double MinimumHeight = 0;

    #endregion

    #region Properties

    #region CloseOnMouseLeave

    public static readonly DependencyProperty CloseOnMouseLeaveProperty = DependencyProperty.RegisterAttached("CloseOnMouseLeave", typeof(bool), typeof(XPopup), new FrameworkPropertyMetadata(false, OnCloseOnMouseLeaveChanged));
    public static bool GetCloseOnMouseLeave(Popup i) => (bool)i.GetValue(CloseOnMouseLeaveProperty);
    public static void SetCloseOnMouseLeave(Popup i, bool input) => i.SetValue(CloseOnMouseLeaveProperty, input);

    private static void OnCloseOnMouseLeaveChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Popup popup)
            popup.AddHandlerAttached((bool)e.NewValue, CloseOnMouseLeaveProperty, i => i.MouseLeave += CloseOnMouseLeave_MouseLeave, i => i.MouseLeave -= CloseOnMouseLeave_MouseLeave);
    }

    private static void CloseOnMouseLeave_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Popup popup)
            popup.SetCurrentValue(Popup.IsOpenProperty, false);
    }

    #endregion

    #region Parent

    public static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(DependencyObject), typeof(XPopup), new FrameworkPropertyMetadata(null, OnParentChanged));
    public static DependencyObject GetParent(Popup i) => (DependencyObject)i.GetValue(ParentProperty);
    public static void SetParent(Popup i, DependencyObject input) => i.SetValue(ParentProperty, input);

    private static void OnParentChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Popup i)
        {
            if (e.NewValue is DependencyObject j)
                XDependency.SetPopup(j, i);
        }
    }

    #endregion

    #endregion
}