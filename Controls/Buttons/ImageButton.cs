using Ion.Data;
using Ion.Numeral;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public class ImageButton : Button
{
    public static readonly DependencyProperty ButtonMarginProperty = DependencyProperty.Register(nameof(ButtonMargin), typeof(Thickness), typeof(ImageButton), new FrameworkPropertyMetadata(default(Thickness)));
    public Thickness ButtonMargin
    {
        get => (Thickness)GetValue(ButtonMarginProperty);
        set => SetValue(ButtonMarginProperty, value);
    }

    public static readonly DependencyProperty ButtonSizeProperty = DependencyProperty.Register(nameof(ButtonSize), typeof(MSize<double>), typeof(ImageButton), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(DoubleSizeTypeConverter))]
    public MSize<double> ButtonSize
    {
        get => (MSize<double>)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public static readonly DependencyProperty ButtonSourceProperty = DependencyProperty.Register(nameof(ButtonSource), typeof(ImageSource), typeof(ImageButton), new FrameworkPropertyMetadata(null));
    public ImageSource ButtonSource
    {
        get => (ImageSource)GetValue(ButtonSourceProperty);
        set => SetValue(ButtonSourceProperty, value);
    }

    public static readonly DependencyProperty ButtonToolTipProperty = DependencyProperty.Register(nameof(ButtonToolTip), typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata(null));
    public string ButtonToolTip
    {
        get => (string)GetValue(ButtonToolTipProperty);
        set => SetValue(ButtonToolTipProperty, value);
    }

    public static readonly DependencyProperty ButtonVisibilityProperty = DependencyProperty.Register(nameof(ButtonVisibility), typeof(Visibility), typeof(ImageButton), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public Visibility ButtonVisibility
    {
        get => (Visibility)GetValue(ButtonVisibilityProperty);
        set => SetValue(ButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof(ContentMargin), typeof(Thickness), typeof(ImageButton), new FrameworkPropertyMetadata(default(Thickness)));
    public Thickness ContentMargin
    {
        get => (Thickness)GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    public static readonly DependencyProperty ContentVisibilityProperty = DependencyProperty.Register(nameof(ContentVisibility), typeof(Visibility), typeof(ImageButton), new FrameworkPropertyMetadata(Visibility.Visible));
    public Visibility ContentVisibility
    {
        get => (Visibility)GetValue(ContentVisibilityProperty);
        set => SetValue(ContentVisibilityProperty, value);
    }

    public static readonly DependencyProperty HasMenuProperty = DependencyProperty.Register(nameof(HasMenu), typeof(bool), typeof(ImageButton), new FrameworkPropertyMetadata(false));
    public bool HasMenu
    {
        get => (bool)GetValue(HasMenuProperty);
        set => SetValue(HasMenuProperty, value);
    }

    public static readonly DependencyProperty ImageForegroundProperty = ImageElement.ForegroundProperty.AddOwner(typeof(ImageButton), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits));
    public Brush ImageForeground
    {
        get => (Brush)GetValue(ImageForegroundProperty);
        set => SetValue(ImageForegroundProperty, value);
    }

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(ImageButton), new FrameworkPropertyMetadata(false));
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(nameof(Menu), typeof(ContextMenu), typeof(ImageButton), new FrameworkPropertyMetadata(null, OnMenuChanged));
    public ContextMenu Menu
    {
        get => (ContextMenu)GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    private static void OnMenuChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<ImageButton>().OnMenuChanged(e.Convert<ContextMenu>());

    public static readonly DependencyProperty MenuAnimationProperty = DependencyProperty.Register(nameof(MenuAnimation), typeof(PopupAnimation), typeof(ImageButton), new FrameworkPropertyMetadata(PopupAnimation.Fade));
    public PopupAnimation MenuAnimation
    {
        get => (PopupAnimation)GetValue(MenuAnimationProperty);
        set => SetValue(MenuAnimationProperty, value);
    }

    public static readonly DependencyProperty MenuPlacementProperty = DependencyProperty.Register(nameof(MenuPlacement), typeof(PlacementMode), typeof(ImageButton), new FrameworkPropertyMetadata(PlacementMode.Bottom));
    public PlacementMode MenuPlacement
    {
        get => (PlacementMode)GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ImageButton), new FrameworkPropertyMetadata(null));
    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceSizeProperty = DependencyProperty.Register(nameof(SourceSize), typeof(MSize<double>), typeof(ImageButton), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(DoubleSizeTypeConverter))]
    public MSize<double> SourceSize
    {
        get => (MSize<double>)GetValue(SourceSizeProperty);
        set => SetValue(SourceSizeProperty, value);
    }

    ///

    public ImageButton() : base() { }

    ///

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        if (Menu != null)
        {
            e.Handled = true;
            SetCurrentValue(IsCheckedProperty, true);
        }
    }

    ///

    protected virtual void OnMenuChanged(ValueChange<ContextMenu> input)
    {
        if (input.NewValue != null)
        {
            input.NewValue.Placement
                = MenuPlacement;
            input.NewValue.PlacementTarget
                = this;
            input.NewValue.Bind(ContextMenu.IsOpenProperty, nameof(IsChecked), this, BindingMode.TwoWay);
        }
        SetCurrentValue(HasMenuProperty, input.NewValue is not null);
    }
}