using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Ion.Controls;

/// <summary>Specifies a single element.</summary>
[ContentProperty(nameof(Child))]
public class Element() : FrameworkElement()
{
    private static readonly FrameworkPropertyMetadata ChildMetadata = new(null, FrameworkPropertyMetadataOptions.AffectsParentArrange, new PropertyChangedCallback(OnChildChanged));
    public static readonly DependencyProperty ChildProperty = DependencyProperty.RegisterAttached(nameof(Child), typeof(UIElement), typeof(Element), ChildMetadata);
    public UIElement Child
    {
        get => (UIElement)GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    private static void OnChildChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Element element)
        {
            if (e.OldValue is Visual oldChild)
            {
                element.RemoveVisualChild(oldChild);
                element.RemoveLogicalChild(oldChild);
            }

            if (e.NewValue is Visual newChild)
            {
                element.AddVisualChild(newChild);
                element.AddLogicalChild(newChild);
                element.OnChildChanged(e.Convert<Visual>());
            }
        }
    }

    public static readonly DependencyProperty ImageForegroundProperty = ImageElement.ForegroundProperty.AddOwner(typeof(Element), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits));
    public Brush ImageForeground
    {
        get => (Brush)GetValue(ImageForegroundProperty);
        set => SetValue(ImageForegroundProperty, value);
    }

    ///

    protected override int VisualChildrenCount => GetValue(ChildProperty) is not null ? 1 : 0;

    protected override Visual GetVisualChild(int index) => (UIElement)GetValue(ChildProperty);

    ///

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (GetValue(ChildProperty) is UIElement child)
            child.Arrange(new Rect(new Point(0, 0), finalSize));

        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (GetValue(ChildProperty) is UIElement child)
        {
            child.Measure(availableSize);
            return child.DesiredSize;
        }
        return new Size(0, 0);
    }

    protected virtual void OnChildChanged(ValueChange<Visual> input) { }
}