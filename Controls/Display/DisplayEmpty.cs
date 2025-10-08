using Ion.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ion.Controls;

public class DisplayEmpty : Display<ItemsControl>
{
    public static readonly ResourceKey PopupMarginKey = new();
    private static readonly DependencyPropertyKey IsEmptyKey = DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(DisplayEmpty), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyKey.DependencyProperty;
    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        private set => SetValue(IsEmptyKey, value);
    }

    private static readonly DependencyProperty IsEmptyChangedProperty = DependencyProperty.Register(nameof(IsEmptyChanged), typeof(bool), typeof(DisplayEmpty), new FrameworkPropertyMetadata(false, OnIsEmptyChanged));

    private bool IsEmptyChanged
    {
        get => (bool)GetValue(IsEmptyChangedProperty);
        set => SetValue(IsEmptyChangedProperty, value);
    }

    private static void OnIsEmptyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DisplayEmpty i)
            i.IsEmpty = (bool)e.NewValue;
    }

    ///

    public DisplayEmpty() : base() { }

    ///

    protected override void OnLoaded()
    {
        base.OnLoaded();
        this.Bind(DisplayEmpty.ContentTemplateProperty,
            new PropertyPath("(0)", XItemsControl.EmptyTemplateProperty),
            Control);
        this.Bind(DisplayEmpty.IsEmptyChangedProperty,
            new PropertyPath("(0)", XItemsControl.IsEmptyProperty),
            Control);

        this.MultiBind(DisplayEmpty.VisibilityProperty, new MultiBindTrue() { Result = MultiBindResult.Results.Visibility }, new Binding() { Path = new PropertyPath("(0)", XItemsControl.EmptyTemplateVisibilityProperty), Source = Control }, new Binding() { Path = new PropertyPath("(0)", XItemsControl.IsEmptyProperty), Source = Control });
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();
        this.Unbind(DisplayEmpty.ContentTemplateProperty); this.Unbind(DisplayEmpty.IsEmptyProperty);
        this.Unbind(DisplayEmpty.VisibilityProperty);
    }
}