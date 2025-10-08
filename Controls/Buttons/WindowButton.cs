using Ion.Data;
using Ion.Numeral;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Controls;

public class WindowButton : Button
{
    public static readonly DependencyProperty ContentSizeProperty = DependencyProperty.Register(nameof(ContentSize), typeof(MSize<double>), typeof(WindowButton), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(DoubleSizeTypeConverter))]
    public MSize<double> ContentSize
    {
        get => (MSize<double>)GetValue(ContentSizeProperty);
        set => SetValue(ContentSizeProperty, value);
    }

    public static readonly DependencyProperty IsCheckableProperty = DependencyProperty.Register(nameof(IsCheckable), typeof(bool), typeof(WindowButton), new FrameworkPropertyMetadata(false));
    public bool IsCheckable
    {
        get => (bool)GetValue(IsCheckableProperty);
        set => SetValue(IsCheckableProperty, value);
    }

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(WindowButton), new FrameworkPropertyMetadata(false, null, OnIsCheckedCoerced));
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private static object OnIsCheckedCoerced(DependencyObject d, object value) => !(d as WindowButton).IsCheckable ? false : value ?? false;

    public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(nameof(Menu), typeof(ContextMenu), typeof(WindowButton), new FrameworkPropertyMetadata(null, OnMenuChanged));
    public ContextMenu Menu
    {
        get => (ContextMenu)GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    private static void OnMenuChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<WindowButton>().OnMenuChanged(e.Convert<ContextMenu>());

    public WindowButton() : base() { }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        if (IsCheckable)
            SetCurrentValue(IsCheckedProperty, !IsChecked);
    }

    protected virtual void OnMenuChanged(ValueChange<ContextMenu> input)
    {
        if (input.NewValue != null)
        {
            input.NewValue.PlacementTarget
                = this;
            input.NewValue.Placement
                = PlacementMode.Bottom;
            input.NewValue.Bind(ContextMenu.IsOpenProperty, nameof(IsChecked), this, BindingMode.TwoWay);
        }
    }
}