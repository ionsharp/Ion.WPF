using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ion.Controls;

public class ClipBorder : Border
{
    private readonly RectangleGeometry clip = new();
    private object oldClip;

    public override UIElement Child
    {
        get => base.Child;
        set
        {
            if (Child != value)
            {
                Child?.SetValue(ClipProperty, oldClip);

                oldClip = value?.ReadLocalValue(ClipProperty);
                base.Child = value;
            }
        }
    }

    public ClipBorder() : base() { }

    protected override void OnRender(DrawingContext context)
    {
        ApplyClip();
        base.OnRender(context);
    }

    private void ApplyClip()
    {
        if (Child != null)
        {
            clip.RadiusX = clip.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - (BorderThickness.Left * 0.5));
            clip.Rect = new Rect(Child.RenderSize);
            Child.Clip = clip;
        }
    }
}