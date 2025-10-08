using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class BinaryPanel() : Panel()
{
    public static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register(nameof(HorizontalContentAlignment), typeof(HorizontalAlignment), typeof(BinaryPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orient), typeof(BinaryPanel), new FrameworkPropertyMetadata(Orient.Horizontal)); //FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
    public Orient Orientation
    {
        get => (Orient)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty PinProperty = DependencyProperty.Register(nameof(Pin), typeof(Sides), typeof(BinaryPanel), new FrameworkPropertyMetadata(Sides.LeftOrTop)); //FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
    public Sides Pin
    {
        get => (Sides)GetValue(PinProperty);
        set => SetValue(PinProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(BinaryPanel), new FrameworkPropertyMetadata(0.0)); //FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyProperty.Register(nameof(VerticalContentAlignment), typeof(VerticalAlignment), typeof(BinaryPanel), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
    public VerticalAlignment VerticalContentAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children?.Count == 2)
        {
            UIElement a = Children[0],
                      b = Children[1];

            Size sA = a.DesiredSize,
                 sB = b.DesiredSize;

            double xA = 0, xB = 0,
                   yA = 0, yB = 0,
                    s = Spacing;

            switch (Orientation)
            {
                case Orient.Horizontal:

                    switch (HorizontalContentAlignment)
                    {
                        case HorizontalAlignment.Left:
                            break;

                        case HorizontalAlignment.Center:
                            break;

                        case HorizontalAlignment.Stretch:
                            sB.Width = finalSize.Width - a.DesiredSize.Width - (a.DesiredSize.Width > 0 ? s : 0);
                            break;

                        case HorizontalAlignment.Right:
                            break;
                    }
                    switch (VerticalContentAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            yA = finalSize.Height - a.DesiredSize.Height;
                            yB = finalSize.Height - b.DesiredSize.Height;
                            break;

                        case VerticalAlignment.Center:
                            yA = (finalSize.Height / 2.0) - (a.DesiredSize.Height / 2.0);
                            yB = (finalSize.Height / 2.0) - (b.DesiredSize.Height / 2.0);
                            break;

                        case VerticalAlignment.Stretch:
                            yA = 0;
                            yB = 0;

                            sA.Height = sB.Height = finalSize.Height;
                            break;

                        case VerticalAlignment.Top:
                            yA = 0;
                            yB = 0;
                            break;
                    }
                    switch (Pin)
                    {
                        case Sides.LeftOrTop:
                            a.Arrange(new Rect(new(0, yA), sA));
                            b.Arrange(new Rect(new(a.DesiredSize.Width > 0 ? a.DesiredSize.Width + s : 0, yB), sB));
                            break;

                        case Sides.RightOrBottom:
                            a.Arrange(new Rect(new(b.DesiredSize.Width > 0 ? b.DesiredSize.Width + s : 0, yA), sA));
                            b.Arrange(new Rect(new(0, yB), sB));
                            break;
                    }
                    break;
                case Orient.Vertical:

                    switch (HorizontalContentAlignment)
                    {
                        case HorizontalAlignment.Left:
                            xA = 0;
                            xB = 0;
                            break;

                        case HorizontalAlignment.Center:
                            xA = (finalSize.Width / 2.0) - (a.DesiredSize.Width / 2.0);
                            xB = (finalSize.Width / 2.0) - (b.DesiredSize.Width / 2.0);
                            break;

                        case HorizontalAlignment.Stretch:
                            xA = 0;
                            xB = 0;

                            sA.Width = sB.Width = finalSize.Width;
                            break;

                        case HorizontalAlignment.Right:
                            xA = finalSize.Width - a.DesiredSize.Width;
                            xB = finalSize.Width - b.DesiredSize.Width;
                            break;
                    }
                    switch (VerticalContentAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            break;

                        case VerticalAlignment.Center:
                            break;

                        case VerticalAlignment.Stretch:
                            break;

                        case VerticalAlignment.Top:
                            break;
                    }
                    switch (Pin)
                    {
                        case Sides.LeftOrTop:
                            a.Arrange(new Rect(new(xA, 0), sA));
                            b.Arrange(new Rect(new(xB, a.DesiredSize.Height > 0 ? a.DesiredSize.Height + s : 0), sB));
                            break;

                        case Sides.RightOrBottom:
                            a.Arrange(new Rect(new(xA, b.DesiredSize.Height > 0 ? b.DesiredSize.Height + s : 0), sA));
                            b.Arrange(new Rect(new(xB, 0), sB));
                            break;
                    }
                    break;
            }
        }
        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size result = new(0, 0);
        if (Children?.Count == 2)
        {
            switch (Orientation)
            {
                case Orient.Horizontal:
                    result = new(Spacing, 0);
                    break;

                case Orient.Vertical:
                    result = new(0, Spacing);
                    break;
            }
            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                switch (Orientation)
                {
                    case Orient.Horizontal:
                        result.Height = result.Height > child.DesiredSize.Height ? result.Height : child.DesiredSize.Height;
                        result.Width += child.DesiredSize.Width;
                        break;

                    case Orient.Vertical:
                        result.Height += child.DesiredSize.Height;
                        result.Width = result.Width > child.DesiredSize.Width ? result.Width : child.DesiredSize.Width;
                        break;
                }
            }
        }
        return result;
    }
}