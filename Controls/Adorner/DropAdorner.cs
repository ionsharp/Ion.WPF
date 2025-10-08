using Ion.Data;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class DropAdorner : Adorner<Control>
{
    private readonly ContentControl Target;

    public DropAdorner(Control control) : base(control)
    {
        Target = new()
        {
            Content = new(),
            IsHitTestVisible = false
        };
        Target.Bind(ContentControl.ContentTemplateProperty,
            new PropertyPath("(0)", XControl.DropTemplateProperty), control);
        Target.Bind(ContentControl.VisibilityProperty,
            new PropertyPath("(0)", XControl.IsDraggingOverProperty), control, System.Windows.Data.BindingMode.OneWay, ValueConverter.Cache.Get<ConvertBooleanToVisibility>());

        Element.Bind(Control.OpacityProperty,
            new PropertyPath("(0)", XControl.IsDraggingOverProperty), control, System.Windows.Data.BindingMode.OneWay, new ValueConverter<bool, double>(i => i.Value ? 0 : 1));

        Children.Add(Target);
        SetCurrentValue(IsHitTestVisibleProperty, false);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Target?.Arrange(new Rect(new Point(0, 0), new Size(Element.ActualWidth, Element.ActualHeight)));
        return finalSize;
    }
}